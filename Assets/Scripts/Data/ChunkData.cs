using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    int x;
    int y;

    public Vector2Int position
    {
        get { return new Vector2Int(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }


    public ChunkData(Vector2Int pos)
    {
        position = pos;
    }
    public ChunkData(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    [System.NonSerialized] public Chunk chunk;

    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public void Populate()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    Vector3 voxelGlobalPos = new Vector3(x + position.x, y, z + position.y);

                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(new Vector3(x + position.x, y, z + position.y)), this, new Vector3Int(x, y, z));

                    for (int p = 0; p < 6; ++p)
                    {
                        Vector3Int neighbourPos = new Vector3Int(x, y, z) + VoxelData.faceChecks[p];
                        if (IsVoxelInChunk(neighbourPos)) map[x, y, z].neighbours[p] = VoxelFromV3Int(neighbourPos);
                        else map[x, y, z].neighbours[p] = World.Instance.worldData.GetVoxel(voxelGlobalPos + VoxelData.faceChecks[p]);
                    }
                }
            }
        }
        Lighting.RecalculateNaturalLight(this);
        World.Instance.worldData.AddToModifiedChunkList(this);
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        return true;
    }

    public VoxelState GetHighestSolidVoxel(int x, int z)
    {
        for (int y = VoxelData.ChunkHeight - 1; y > -1; --y)
        {
            if (IsVoxelInChunk(x, y, z) && World.Instance.blockTypes[map[x, y, z].id].isSolid)
            {
                return map[x, y, z];
            }
        }
        return null;
    }

    private bool IsPlacingStairs(byte _id)
    {
        return _id == (byte)VoxelBlockID.OAK_STAIRS || _id == (byte)VoxelBlockID.COBBLESTONE_STAIRS;
    }

    private bool IsSameStairsType(VoxelState block, VoxelState neighbour)
    {
        return IsPlacingStairs(block.id) && neighbour.id - block.id <= 4 && neighbour.id - block.id >= 0;
    }

    private int TranslateToAngles(int orientation)
    {
        if (orientation == 0) return 180;
        if (orientation == 5) return 90;
        if (orientation == 4) return 270;

        return 0;
    }

    private byte GetCorrectStairsType(VoxelState voxel)
    {
        if (IsSameStairsType(voxel, voxel.neighbours[VoxelData.orientedFaceChecks[voxel.orientation][0]]))
        {
            VoxelState backNeighbour = voxel.neighbours[VoxelData.orientedFaceChecks[voxel.orientation][0]];
            int neighToVoxelDiff = TranslateToAngles(backNeighbour.orientation) - TranslateToAngles(voxel.orientation);
            int voxelToNeighDiff = TranslateToAngles(voxel.orientation) - TranslateToAngles(backNeighbour.orientation);

            if (backNeighbour.id == (byte)VoxelBlockID.OAK_STAIRS || backNeighbour.id == (byte)VoxelBlockID.COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 90 || neighToVoxelDiff == -270)
                {
                    return (byte)(voxel.id + 2);
                }

                if (voxelToNeighDiff == 90 || voxelToNeighDiff == -270)
                {
                    return (byte)(voxel.id + 1);
                }
            }

            if (backNeighbour.id == (byte)VoxelBlockID.LEFT_CORNER_OAK_STAIRS || backNeighbour.id == (byte)VoxelBlockID.LEFT_CORNER_COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 90 || neighToVoxelDiff == -270)
                {
                    return (byte)(voxel.id + 2);
                }

                if (voxelToNeighDiff == 180 || voxelToNeighDiff == -180)
                {
                    return (byte)(voxel.id + 1);
                }
            }

            if (backNeighbour.id == (byte)VoxelBlockID.RIGHT_CORNER_OAK_STAIRS || backNeighbour.id == (byte)VoxelBlockID.RIGHT_CORNER_COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 180 || neighToVoxelDiff == -180)
                {
                    return (byte)(voxel.id + 2);
                }

                if (voxelToNeighDiff == 90 || voxelToNeighDiff == -270)
                {
                    return (byte)(voxel.id + 1);
                }
            }
        }

        if (IsSameStairsType(voxel, voxel.neighbours[VoxelData.orientedFaceChecks[voxel.orientation][1]]))
        {
            VoxelState frontNeighbour = voxel.neighbours[VoxelData.orientedFaceChecks[voxel.orientation][1]];
            int neighToVoxelDiff = TranslateToAngles(frontNeighbour.orientation) - TranslateToAngles(voxel.orientation);
            int voxelToNeighDiff = TranslateToAngles(voxel.orientation) - TranslateToAngles(frontNeighbour.orientation);

            if (frontNeighbour.id == (byte)VoxelBlockID.OAK_STAIRS || frontNeighbour.id == (byte)VoxelBlockID.COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 90 || neighToVoxelDiff == -270)
                {
                    return (byte)(voxel.id + 4);
                }

                if (voxelToNeighDiff == 90 || voxelToNeighDiff == -270)
                {
                    return (byte)(voxel.id + 3);
                }
            }

            if (frontNeighbour.id == (byte)VoxelBlockID.LEFT_TRIPLE_CORNER_OAK_STAIRS || frontNeighbour.id == (byte)VoxelBlockID.LEFT_TRIPLE_CORNER_COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 90 || neighToVoxelDiff == -270)
                {
                    return (byte)(voxel.id + 4);
                }

                if (voxelToNeighDiff == 180 || voxelToNeighDiff == -180)
                {
                    return (byte)(voxel.id + 3);
                }
            }

            if (frontNeighbour.id == (byte)VoxelBlockID.RIGHT_TRIPLE_CORNER_OAK_STAIRS || frontNeighbour.id == (byte)VoxelBlockID.RIGHT_TRIPLE_CORNER_COBBLESTONE_STAIRS)
            {
                if (neighToVoxelDiff == 180 || neighToVoxelDiff == -180)
                {
                    return (byte)(voxel.id + 4);
                }

                if (voxelToNeighDiff == 90 || voxelToNeighDiff == -270)
                {
                    return (byte)(voxel.id + 3);
                }
            }
        }

        return voxel.id;
    }

    public void ModifyVoxel(Vector3Int pos, byte _id, int direction)
    {
        if (map[pos.x, pos.y, pos.z].id == _id) return;

        VoxelState voxel = map[pos.x, pos.y, pos.z];
        BlockType oldVoxel = World.Instance.blockTypes[voxel.id];
        BlockType newVoxel = World.Instance.blockTypes[_id];

        byte oldOpacity = voxel.properties.opacity;

        voxel.id = _id;

        voxel.orientation = direction;

        if (newVoxel.isContainer)
        {
            if (oldVoxel.itemID != newVoxel.itemID)
            {
                int containerLength = _id == (byte)VoxelBlockID.CHEST ? 27 :
                 (
                     _id == (byte)VoxelBlockID.FURNANCE ? 3 : 
                     (
                        _id == (byte)VoxelBlockID.CRAFTING_TABLE ? 10 : 0
                     )
                 );
                voxel.itemsContained = new ItemStack[containerLength];
            }
        }
        else voxel.itemsContained = null;

        if (IsPlacingStairs(voxel.id))
        {
            voxel.id = GetCorrectStairsType(voxel);
        }

        if (voxel.properties.opacity != oldOpacity && (pos.y == VoxelData.ChunkHeight - 1 || (pos.y < VoxelData.ChunkHeight - 1 && map[pos.x, pos.y + 1, pos.z].light == 15))) {
            Lighting.CastNaturalLight(this, pos.x, pos.z, pos.y + 1);
        }

        if (voxel.properties.isActive && BlockBehaviour.Active(voxel) && voxel.chunkData != null && voxel.chunkData.chunk != null)
        {
            voxel.chunkData.chunk.AddActiveVoxel(voxel);
        }
        for (int i = 0; i < 6; ++i)
        {
            if (voxel.neighbours[i] != null)
            {
                if (voxel.neighbours[i].properties.isActive && BlockBehaviour.Active(voxel.neighbours[i]) && voxel.neighbours[i].chunkData != null && voxel.neighbours[i].chunkData.chunk != null)
                {
                    voxel.neighbours[i].chunkData.chunk.AddActiveVoxel(voxel.neighbours[i]);
                }
            }
        }

        World.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            World.Instance.AddChunkToUpdate(chunk);
            chunk.UpdateSpawnableList(_id, pos);
        }

    }

    public bool IsVoxelInChunk(Vector3Int pos)
    {
        return IsVoxelInChunk(pos.x, pos.y, pos.z);
    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}
