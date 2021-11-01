using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;

    [Header("World Generation Values")]
    public BiomeAttributes[] biomes;

    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    public List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    public GameObject debugScreen;

    public bool applyModifications;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool _inUI = false;

    public GameObject creativeInventoryMenu;
    public GameObject cursorSlot;

    Thread chunkUpdateThread;
    public object chunkUpdateThreadLock = new object();

    private void Start()
    {
        /*string jsonExport = JsonUtility.ToJson(settings);
        Debug.Log(jsonExport);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);*/

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(settings.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (settings.enableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }
        SetGlobalLightValue();
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }


        if (!applyModifications)
        {
            ApplyModifications();
        }

        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }
        if (chunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }
        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
            {
                chunksToDraw.Dequeue().CreateMesh();
            }   
        }

        if (!settings.enableThreading)
        {
            if (!applyModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    void GenerateWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - settings.viewDistance; x < VoxelData.WorldSizeInChunks / 2 + settings.viewDistance; ++x)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - settings.viewDistance; z < VoxelData.WorldSizeInChunks / 2 + settings.viewDistance; ++z)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);
            }
        }

        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord coord = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);

        chunks[coord.x, coord.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock (chunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if (!activeChunks.Contains(chunksToUpdate[index].coord))
                    {
                        activeChunks.Add(chunksToUpdate[index].coord);
                    }
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ApplyModifications()
    {
        applyModifications = true;

        while(modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod vMod = queue.Dequeue();

                ChunkCoord coord = GetChunkCoordFromVector3(vMod.position);
                if (chunks[coord.x, coord.z] == null)
                {
                    chunks[coord.x, coord.z] = new Chunk(coord, this);
                    chunksToCreate.Add(coord);
                }
                chunks[coord.x, coord.z].modifications.Enqueue(vMod);

            }
        }

        applyModifications = false;
    }


    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return chunks[x, z];
    }


    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;


        List<ChunkCoord> prevActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; ++x)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; ++z)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                if (IsChunkInWorld(thisChunkCoord))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    } 
                    else
                    {
                        if (!chunks[x, z].isActive)
                        {
                            chunks[x, z].isActive = true;
                        }
                    }
                    activeChunks.Add(thisChunkCoord);

                }

                for (int i = 0; i < prevActiveChunks.Count; ++i)
                {
                    if (prevActiveChunks[i].Equals(thisChunkCoord))
                    {
                        prevActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach(ChunkCoord prevCoord in prevActiveChunks)
        {
            chunks[prevCoord.x, prevCoord.z].isActive = false;
        }
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        return false;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        /* IMMUTABLE PASS */
        if (!IsVoxelInWorld(pos))
            return 0;
        if (yPos == 0)
            return 1;

        /* BIOME SELECTION PASS */

        int solidGroundHeight = 64;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; ++i)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

        /* BASIC TERRAIN PASS */

        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else
        {
            if (yPos < terrainHeight && yPos > terrainHeight - 4)
            {
                voxelValue = biome.subSurfaceBlock;
            }
            else
            {
                if (yPos < terrainHeight)
                {
                    voxelValue = 2;
                }
                else
                {
                    return 0;
                }

            }
        }

        /* LODE PASS */
        if (voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        /* MAJOR FLORA PASS */
        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 300, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 700, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minMajorFloraHeight, biome.maxMajorFloraHeight));
                }
            }
        }



        return voxelValue;

    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsVoxelInWorld(pos) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsVoxelInWorld(pos) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return null;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }

        return new VoxelState(GetVoxel(pos));
    }


    public bool inUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryMenu.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryMenu.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }


    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            chunkUpdateThread.Abort();
        }
    }


}


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool renderNeighbourFaces;
    public float transparency;
    public Sprite icon;
    public int maxItemStack;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right
    public int GetTextureID (int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID");
                return -1;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;


    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }
    public VoxelMod (Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version;

    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;
    public bool enableAnimatedChunks;

    [Header("Controls")]
    [Range(0.5f, 20f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int seed;
}
