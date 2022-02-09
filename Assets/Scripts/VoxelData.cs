using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 100;
    public static readonly int WorldWaterLevel = 63;
    public static readonly float GetExtraDropChance = 0.3f;

    // Tick Updates Values
    public static readonly float TickLength = 0.1f;
    public static readonly float GrassSpreadSpeed = 0.0075f;
    public static readonly float SaplingGrowthChance = 0.003f;
    public static readonly float FarmlandTranformChance = 0.0075f;
    public static readonly float SeedGrowOnDry = 0.001f;
    public static readonly float SeedGrowOnWet = 0.005f;

    // Lighting Values
    public static readonly float minLightLevel = 0.005f;
    public static readonly float maxLightLevel = 0.9f;

    // Flora Growth Limits
    public static readonly int OakMaxHeight = 7;


    public static float unitOfLight
    {
        get { return 0.0625f; } // 1 / 16 (Level of light)
    }

    public static int seed;

    public static int WorldCenter
    {
        get { return (WorldSizeInChunks * ChunkWidth) / 2; }
    }

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f), // 0
        new Vector3(1.0f, 0.0f, 0.0f), // 1
        new Vector3(1.0f, 1.0f, 0.0f), // 2
        new Vector3(0.0f, 1.0f, 0.0f), // 3
        new Vector3(0.0f, 0.0f, 1.0f), // 4
        new Vector3(1.0f, 0.0f, 1.0f), // 5
        new Vector3(1.0f, 1.0f, 1.0f), // 6
        new Vector3(0.0f, 1.0f, 1.0f), // 7
    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
    };

    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static readonly Dictionary<int, Dictionary<int, int>> orientedFaceChecks = new Dictionary<int, Dictionary<int, int>>()
    {
        {1, new Dictionary<int, int>(){ {0, 0 }, {1, 1 }, { 2, 2 }, { 3, 3 }, {4, 4 }, {5, 5 } } },
        {5, new Dictionary<int, int>(){ {0, 5 }, {1, 4 }, { 2, 2 }, { 3, 3 }, { 4, 0 }, {5, 1 } } },
        {0, new Dictionary<int, int>(){ {0, 1 }, {1, 0 }, { 2, 2 }, { 3, 3 }, { 4, 5 }, {5, 4 } } },
        {4, new Dictionary<int, int>(){ {0, 4 }, {1, 5 }, { 2, 2 }, { 3, 3 }, { 4, 1 }, {5, 0 } } },
    };

    public static readonly int[,] voxelTriangles = new int[6, 4]
    {
        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face
    };

    public static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),
    };
}
