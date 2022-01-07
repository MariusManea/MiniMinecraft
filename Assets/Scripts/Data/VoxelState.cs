using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelState
{
    public byte id;
    public int orientation;
    public ItemStack[] itemsContained;
    [System.NonSerialized] private byte _light;
    [System.NonSerialized] private float _remainingSmeltingTimer;
    [System.NonSerialized] private float _smeltingTimer;
    [System.NonSerialized] private float _maxRemainingSmeltingTimer;

    [System.NonSerialized] public ChunkData chunkData;

    [System.NonSerialized] public VoxelNeighbours neighbours;
    [System.NonSerialized] public Vector3Int position;

    public byte light
    {
        get { return _light; }
        set { 
            if (value != _light)
            {
                byte oldLightValue = _light;
                byte oldCastValue = castLight;

                _light = value;

                if (_light < oldLightValue)
                {
                    List<int> neighboursToDarken = new List<int>();
                    for (int p = 0; p < 6; p++)
                    {
                        if (neighbours[p] != null)
                        {
                            if (neighbours[p].light <= oldCastValue)
                            {
                                neighboursToDarken.Add(p);
                            }
                            else
                            {
                                neighbours[p].PropogateLight();
                            }
                        }
                    }
                    foreach (int i in neighboursToDarken)
                    {
                        neighbours[i].light = 0;
                    }

                    if (chunkData.chunk != null)
                    {
                        World.Instance.AddChunkToUpdate(chunkData.chunk);
                    }
                } 
                else
                {
                    if (_light > 1)
                    {
                        PropogateLight();
                    }
                }
            }
        }
    }

    public float maxRemainingSmeltingTImer
    {
        get { return _maxRemainingSmeltingTimer; }
        set { _maxRemainingSmeltingTimer = value; }
    }
    public float remainingSmeltingTImer
    {
        get { return _remainingSmeltingTimer; }
        set { _remainingSmeltingTimer = value; }
    }
    public float smeltingTimer
    {
        get { return _smeltingTimer; }
        set { _smeltingTimer = value; }
    }

    public VoxelState(byte _id, ChunkData _chunkData, Vector3Int _position)
    {
        id = _id;
        orientation = 1;
        chunkData = _chunkData;
        neighbours = new VoxelNeighbours(this);
        position = _position;
        itemsContained = null;
        light = 0;
    }

    public VoxelState(byte _id, ChunkData _chunkData, Vector3Int _position, ItemStack[] _itemsContained)
    {
        id = _id;
        orientation = 1;
        chunkData = _chunkData;
        neighbours = new VoxelNeighbours(this);
        position = _position;
        itemsContained = _itemsContained;
        light = 0;
    }

    public Vector3Int globalPosition
    {
        get
        {
            return new Vector3Int(position.x + chunkData.position.x, position.y, position.z + chunkData.position.y);
        }
    }

    private bool isIrregularShape()
    {
        switch ((VoxelBlockID)id)
        {
            case VoxelBlockID.OAK_STAIRS:
            case VoxelBlockID.RIGHT_CORNER_OAK_STAIRS:
            case VoxelBlockID.LEFT_CORNER_OAK_STAIRS:
            case VoxelBlockID.RIGHT_TRIPLE_CORNER_OAK_STAIRS:
            case VoxelBlockID.LEFT_TRIPLE_CORNER_OAK_STAIRS:
            case VoxelBlockID.COBBLESTONE_STAIRS:
            case VoxelBlockID.RIGHT_CORNER_COBBLESTONE_STAIRS:
            case VoxelBlockID.LEFT_CORNER_COBBLESTONE_STAIRS:
            case VoxelBlockID.RIGHT_TRIPLE_CORNER_COBBLESTONE_STAIRS:
            case VoxelBlockID.LEFT_TRIPLE_CORNER_COBBLESTONE_STAIRS:
                return true;
        }
        return false;
    }

    public float getHeightAt(float x, float z)
    {
        if (id == (byte)VoxelBlockID.COBBLESTONE_SLAB)
        {
            // any slabs
            return 0.5f;
        }
        if (isIrregularShape())
        {
            // any shape
            VertData[] vertices = World.Instance.blockTypes[id].meshData.faces[2].vertData;
            float minDistance = float.MaxValue;
            float minY = 1;
            foreach(VertData vertex in vertices)
            {
                Vector3 rotatedPosition = vertex.GetRotatedPosition(new Vector3(0, orientation == 0 ? 180f : (orientation == 5 ? 270f : (orientation == 1 ? 0f : 90f)), 0));

                float dist = Mathf.Sqrt(Mathf.Pow(x - (globalPosition.x + rotatedPosition.x), 2) + Mathf.Pow(z - (globalPosition.z + rotatedPosition.z), 2));
                if (dist < minDistance)
                {
                    minDistance = dist;
                    minY = vertex.position.y;
                }
            }
            return minY;
        }
        return 1;
    }

    public float lightAsFloat
    {
        get { return (float)light * VoxelData.unitOfLight; }
    }

    public byte castLight
    {
        get
        {
            int lightLevel = _light - properties.opacity - 1;
            if (lightLevel < 0) lightLevel = 0;
            return (byte)lightLevel;
        }
    }

    public void PropogateLight()
    {
        if (light < 2) return;

        for (int p = 0; p < 6; ++p)
        {
            if (neighbours[p] != null)
            {
                if (neighbours[p].light < castLight)
                    neighbours[p].light = castLight;
            }

            if (chunkData.chunk != null)
            {
                World.Instance.AddChunkToUpdate(chunkData.chunk);
            }
        }
    }

    public ItemStack[] GetItemsInside()
    {
        return itemsContained;
    }

    public void PutItemInsideAt(ItemStack _item, int index)
    {
        if (index < 0 || index >= itemsContained.Length) return;

        itemsContained[index] = _item;
    }

    public BlockType properties
    {
        get { return World.Instance.blockTypes[id]; }
    }
}

public class VoxelNeighbours
{
    public readonly VoxelState parent;
    public VoxelNeighbours (VoxelState _parent) { parent = _parent; }

    public VoxelState[] _neighbours = new VoxelState[6];
    public int Length { get { return _neighbours.Length; } }

    public VoxelState this[int index]
    {
        get {
            if (_neighbours[index] == null)
            {
                _neighbours[index] = World.Instance.worldData.GetVoxel(parent.globalPosition + VoxelData.faceChecks[index]);
                ReturnNeighbour(index);
            }
            return _neighbours[index]; 
        }
        set { 
            _neighbours[index] = value;
            ReturnNeighbour(index);
        }
    }

    void ReturnNeighbour (int index)
    {
        if (_neighbours[index] == null) return;

        if (_neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] != parent)
        {
            _neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] = parent;

        }
    }
}
