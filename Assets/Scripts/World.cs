using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes defaultBiome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    public GameObject debugScreen;

    public bool applyModifications;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool _inUI = false;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
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
            lock (chunksToDraw)
            {
                if (chunksToDraw.Peek().isEditable)
                {
                    chunksToDraw.Dequeue().CreateMesh();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    void GenerateWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks; x < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks; ++x)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks; z < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks; ++z)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        player.position = spawnPosition;
    }

    void CreateChunk()
    {
        ChunkCoord coord = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);

        activeChunks.Add(coord);
        chunks[coord.x, coord.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1)
        {
            if (chunksToUpdate[index].isEditable)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
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
                    chunks[coord.x, coord.z] = new Chunk(coord, this, true);
                    activeChunks.Add(coord);
                }
                chunks[coord.x, coord.z].modifications.Enqueue(vMod);

                if (!chunksToUpdate.Contains(chunks[coord.x, coord.z]))
                {
                    chunksToUpdate.Add(chunks[coord.x, coord.z]);
                }
            }
        }

        applyModifications = false;
    }


    /*IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);

            yield return null;
        }

        isCreatingChunks = false;
    }*/

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

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; ++x)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; ++z)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                        activeChunks.Add(new ChunkCoord(x, z));
                    } 
                    else
                    {
                        if (!chunks[x, z].isActive)
                        {
                            chunks[x, z].isActive = true;
                            activeChunks.Add(new ChunkCoord(x, z));
                        }
                    }
                }

                for(int i = 0; i < prevActiveChunks.Count; ++i)
                {
                    if (prevActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        prevActiveChunks.RemoveAt(i--);
                    }
                }
            }
        }
        foreach(ChunkCoord prevCoord in prevActiveChunks)
        {
            chunks[prevCoord.x, prevCoord.z].isActive = false;
            activeChunks.Remove(new ChunkCoord(prevCoord.x, prevCoord.z));
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

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, defaultBiome.terrainScale) * defaultBiome.terrainHeight) + defaultBiome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = 3;
        }
        else
        {
            if (yPos < terrainHeight && yPos > terrainHeight - 4)
            {
                voxelValue = 6;
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

        /* SECOND PASS */
        if (voxelValue == 2)
        {
            foreach(Lode lode in defaultBiome.lodes)
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

        /* TREE PASS */
        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 300, defaultBiome.treeZoneScale) > defaultBiome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 700, defaultBiome.treePlacementScale) > defaultBiome.treePlacementThreshold)
                {
                    modifications.Enqueue(Structure.MakeTree(pos, defaultBiome.minTreeHeight, defaultBiome.maxTreeHeight));
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
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckForTransparentVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsVoxelInWorld(pos) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;
        }

        return blockTypes[GetVoxel(pos)].isTransparent;
    }


    public bool inUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;

        }
    }

}


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public Sprite icon;

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
