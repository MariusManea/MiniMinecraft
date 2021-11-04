using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public string worldName = "New World";
    public int seed;

    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();

    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new List<ChunkData>();

    public void AddToModifiedChunkList(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
        {
            modifiedChunks.Add(chunk);
        }
    }

    public WorldData (WorldData wd)
    {
        worldName = wd.worldName;
        seed = wd.seed;
    }

    public WorldData(string _worldName, int _seed)
    {
        worldName = _worldName;
        seed = _seed;
    }

    public ChunkData RequestChunk(Vector2Int coord, bool create)
    {
        ChunkData chunk;

        lock (World.Instance.chunkListThreadLock)
        {
            if (chunks.ContainsKey(coord)) chunk = chunks[coord];
            else if (!create) chunk = null;
            else { LoadChunk(coord); chunk = chunks[coord]; }
        }
        return chunk;

    }

    public void LoadChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord)) return;

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null)
        {
            chunks.Add(coord, chunk);
            return;
        }

        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        return false;
    }

    public void SetVoxel(Vector3 pos, byte value)
    {
        if (!IsVoxelInWorld(pos)) return;

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));
        chunk.map[voxel.x, voxel.y, voxel.z].id = value;
        AddToModifiedChunkList(chunk);
    }

    public VoxelState GetVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos)) return null;

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));
        return chunk.map[voxel.x, voxel.y, voxel.z];
    }
}
