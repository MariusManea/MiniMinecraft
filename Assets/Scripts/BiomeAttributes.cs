using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MiniMinecraft/Biome Attributes")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Generation")]
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainHeight;
    public float terrainScale;


    public VoxelBlockID surfaceBlock;
    public VoxelBlockID subSurfaceBlock;

    [Header("Major Flora")]
    public int majorFloraIndex;
    public float majorFloraZoneScale = 1.3f;
    [Range(0f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f;
    public int maxMajorFloraHeight = 12;
    public int minMajorFloraHeight = 5;
    public bool placeMajorFlora = true;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public VoxelBlockID blockID;

    public int minHeight;
    public int maxHeight;

    public float scale;
    public float threshold;
    public float noiseOffset;
}
