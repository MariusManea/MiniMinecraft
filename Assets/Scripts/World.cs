using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using UnityEngine.UI;

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
    public Player _player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material liquidMaterial;
    public BlockType[] blockTypes;
    public Item[] itemTypes;
    public RecipeData[] recipes;
    public Clouds clouds;

    public GameObject[] overworldMobs;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    public GameObject debugScreen;

    public bool applyModifications;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool _inUI = false;
    private bool _inFirstPerson = true;
    private bool _mainUI = false;

    public GameObject creativeInventoryMenu;
    public GameObject survivalInventoryMenu;
    public GameObject chestInventoryMenu;
    public GameObject furnanceInventoryMenu;
    public GameObject armorInventoryMenu;
    public GameObject handCraftInventoryMenu;
    public GameObject craftInventoryMenu;
    public GameObject toolbar;
    public GameObject cursorSlot;

    Thread chunkUpdateThread;
    public object chunkUpdateThreadLock = new object();
    public object chunkListThreadLock = new object();

    public string appPath;

    public WorldData worldData;

    private static World _instance;
    public static World Instance { get { return _instance; } }

    public Image bossHealthBar;
    public Boss boss;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        appPath = Application.persistentDataPath;

        _player = player.GetComponent<Player>();
    }

    private void Start()
    {
        Debug.Log(VoxelData.seed);

        worldData = SaveSystem.LoadWorld("Test World");
        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(VoxelData.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);
       
        LoadWorld();

        SetGlobalLightValue();


        spawnPosition = new Vector3(VoxelData.WorldCenter + 0.5f, VoxelData.ChunkHeight - 10, VoxelData.WorldCenter + 0.5f);
        player.position = spawnPosition;
        CheckViewDistance();

        VoxelState temporaryVoxel = worldData.GetVoxel(new Vector3(VoxelData.WorldCenter, VoxelData.ChunkHeight - 10, VoxelData.WorldCenter));
        if (temporaryVoxel != null)
        {
            VoxelState highestSolid = temporaryVoxel.chunkData.GetHighestSolidVoxel(temporaryVoxel.position.x, temporaryVoxel.position.z);
            if (highestSolid != null)
            {
                spawnPosition.y = highestSolid.position.y + 1;
                player.position = spawnPosition;
            }
        }
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        survivalInventoryMenu.SetActive(false);
        chestInventoryMenu.SetActive(false);
        furnanceInventoryMenu.SetActive(false);
        armorInventoryMenu.SetActive(false);
        handCraftInventoryMenu.SetActive(false);
        craftInventoryMenu.SetActive(false);
        if (settings.enableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }

        StartCoroutine(Tick());
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

        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
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

       /* if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveSystem.SaveWorld(worldData);
        }*/
    }

    void LoadWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - settings.loadDistance; x < VoxelData.WorldSizeInChunks / 2 + settings.loadDistance; ++x)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - settings.loadDistance; z < VoxelData.WorldSizeInChunks / 2 + settings.loadDistance; ++z)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
        }
    }

    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }

    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        lock (chunkListThreadLock)
        {
            if (!chunksToUpdate.Contains(chunk))
            {
                if (insert) chunksToUpdate.Insert(0, chunk);
                else chunksToUpdate.Add(chunk);
            }
        }
    }

    void UpdateChunks()
    {


        lock (chunkUpdateThreadLock)
        {
            if (chunksToUpdate.Count > 0)
            {
                chunksToUpdate[0].UpdateChunk();
                if (!activeChunks.Contains(chunksToUpdate[0].coord))
                {
                    activeChunks.Add(chunksToUpdate[0].coord);
                }
                chunksToUpdate.RemoveAt(0);
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

                worldData.SetVoxel(vMod.position, vMod.id, 1);

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
        clouds.UpdateClouds();

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
                        chunks[x, z] = new Chunk(thisChunkCoord);
                    } 
                    chunks[x, z].isActive = true;
                  
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
            return (byte)VoxelBlockID.AIR_BLOCK;
        if (yPos == 0)
            return (byte)VoxelBlockID.BEDROCK;

        /* BIOME SELECTION PASS */

        int solidGroundHeight = 55;
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
            voxelValue = (byte)biome.surfaceBlock;
        }
        else
        {
            if (yPos < terrainHeight && yPos > terrainHeight - 4)
            {
                voxelValue = (byte)biome.subSurfaceBlock;
            }
            else
            {
                if (yPos < terrainHeight)
                {
                    voxelValue = (byte)VoxelBlockID.STONE_BLOCK;
                }
                else
                {
                    if (yPos < VoxelData.WorldWaterLevel)
                    {
                        return (byte)VoxelBlockID.WATER_BLOCK;
                    }
                    else
                    {
                        return (byte)VoxelBlockID.AIR_BLOCK;
                    }
                }

            }
        }

        /* LODE PASS */
        if (voxelValue == (byte)VoxelBlockID.STONE_BLOCK)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = (byte)lode.blockID;
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

    IEnumerator Tick()
    {
        while(true)
        {
            foreach (ChunkCoord c in activeChunks)
            {
                chunks[c.x, c.z].TickUpdate();
            }


            yield return new WaitForSeconds(VoxelData.TickLength);
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);
        if (voxel != null && blockTypes[voxel.id].isSolid) return true;
        return false;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }

    public void OpenContainer(VoxelState container)
    {
        inUI = true;

        switch (blockTypes[container.id].itemID)
        {
            case ItemID.CHEST:
                chestInventoryMenu.SetActive(true);
                chestInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(container);
                break;
            case ItemID.FURNANCE:
                furnanceInventoryMenu.SetActive(true);
                furnanceInventoryMenu.GetComponent<ContainerInventory>().PopulateContainerUI(container);
                break;
            case ItemID.CRAFTING_TABLE:
                craftInventoryMenu.SetActive(true);
                craftInventoryMenu.transform.GetChild(0).GetComponent<ContainerInventory>().PopulateContainerUI(container);
                craftInventoryMenu.transform.GetChild(2).GetComponent<ContainerInventory>().PopulateContainerUI(container, 1);
                break;
            default:
                break;
        }
    }

    public bool mainUI
    {
        get { return _mainUI; }
        set
        {
            _mainUI = value;
            if (_mainUI)
            {
                GameMode gameMode = player.GetComponent<Player>().gameMode;
                if (gameMode == GameMode.survival)
                {
                    armorInventoryMenu.SetActive(true);
                    handCraftInventoryMenu.SetActive(true);
                }
            }
            else
            {
                armorInventoryMenu.SetActive(false);
                handCraftInventoryMenu.SetActive(false);
            }
        }
    }

    public bool inFirstPerson
    {
        get { return _inFirstPerson; }
        set
        {
            _inFirstPerson = value;
            if (_inFirstPerson)
            {
                _player.FPSCamera.GetComponent<Camera>().enabled = true;
                _player.itemsCamera.enabled = true;
                _player.thirdPersonCamera.enabled = false;
                _player.steve.SetActive(false);
            } 
            else
            {
                _player.FPSCamera.GetComponent<Camera>().enabled = false;
                _player.itemsCamera.enabled = false;
                _player.thirdPersonCamera.enabled = true;
                _player.steve.SetActive(true);
            }
            if (_player.equippedItem)
            {
                _player.EquipItem(_player.equippedItem);
            }
        }
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
                GameMode gameMode = player.GetComponent<Player>().gameMode;
                if (gameMode == GameMode.survival) survivalInventoryMenu.SetActive(true);
                else creativeInventoryMenu.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                mainUI = false;
                creativeInventoryMenu.SetActive(false);
                survivalInventoryMenu.SetActive(false);
                chestInventoryMenu.SetActive(false);
                furnanceInventoryMenu.SetActive(false);
                armorInventoryMenu.SetActive(false);
                handCraftInventoryMenu.SetActive(false);
                craftInventoryMenu.SetActive(false);
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

    public void AddToModificationsList(Queue<VoxelMod> voxelMods)
    {
        modifications.Enqueue(voxelMods);
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
    public bool isLiquid;
    public bool isSpawnable;
    public bool isContainer;
    public VoxelMeshData meshData;
    public bool renderNeighbourFaces;
    public byte opacity;
    public Sprite icon;
    public int maxItemStack;
    public bool isActive;
    public ItemID itemID;
    public ItemID dropItemID;
    public ItemID[] extraDropItemsID;

    public ToolType prefferedTool;
    public ToolType minimumTool;
    public ToolQuality minimumQuality;
    public float breakingTime;

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
    public string version = "0.0.0.1";

    [Header("Performance")]
    public int loadDistance = 16;
    public int viewDistance = 8;
    public bool enableThreading = true;
    public CloudStyle clouds = CloudStyle.Fast;
    public bool enableAnimatedChunks = false;

    [Header("Controls")]
    [Range(0.5f, 20f)]
    public float mouseSensitivity = 2.0f;
}
