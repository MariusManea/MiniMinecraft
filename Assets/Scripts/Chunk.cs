using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;
    GameObject chunkObject;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    List<int> liquidTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Material[] materials = new Material[3];
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    private bool _isActive;

    List<VoxelState> activeVoxels = new List<VoxelState>();
    List<VoxelState> spawnableVoxels = new List<VoxelState>();

    public Vector3 position;
    ChunkData chunkData;

    private int spawnEveryNTick = (int)(5 / VoxelData.TickLength);

    public Chunk (ChunkCoord _coord)
    {
        coord = _coord;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        materials[0] = World.Instance.material;
        materials[1] = World.Instance.transparentMaterial;
        materials[2] = World.Instance.liquidMaterial;
        meshRenderer.materials = materials;
        chunkObject.transform.parent = World.Instance.transform;
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "chunk_x_" + coord.x + "_z_" + coord.z;
        chunkObject.layer = LayerMask.NameToLayer("Terrain");
        position = chunkObject.transform.position;

        chunkData = World.Instance.worldData.RequestChunk(new Vector2Int((int)position.x, (int)position.z), true);
        chunkData.chunk = this;

        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    VoxelState voxel = chunkData.map[x, y, z];
                    if (voxel.properties.isActive) AddActiveVoxel(voxel);
                    if (voxel.properties.isSpawnable)
                    {
                        if ((y == VoxelData.ChunkHeight - 1) ||
                            (y == VoxelData.ChunkHeight - 2 && chunkData.map[x, y + 1, z].id == (byte)VoxelBlockID.AIR_BLOCK) ||
                            (chunkData.map[x, y + 1, z].id == (byte)VoxelBlockID.AIR_BLOCK && chunkData.map[x, y + 2, z].id == (byte)VoxelBlockID.AIR_BLOCK))
                        {
                            AddSpawnableVoxel(voxel);
                        }
                    }
                }
            }
        }

        World.Instance.AddChunkToUpdate(this);

        if (World.Instance.settings.enableAnimatedChunks)
        {
            chunkObject.AddComponent<ChunkLoadAnimation>();
        }
        
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return chunkData.map[xCheck, yCheck, zCheck];
    }

    public void TickUpdate()
    {
        for (int i = activeVoxels.Count - 1; i > -1; --i)
        {
            if (!BlockBehaviour.Active(activeVoxels[i]))
            {
                RemoveActiveVoxel(activeVoxels[i]);
            }
            else
            {
                BlockBehaviour.Behave(activeVoxels[i]);
            }
        }

        spawnEveryNTick--;
        if (spawnEveryNTick < 0)
        {
            spawnEveryNTick = (int)(5 / VoxelData.TickLength);
            for (int i = spawnableVoxels.Count - 1; i > -1; --i)
            {
                if (!BlockBehaviour.Spawnable(spawnableVoxels[i]))
                {
                    RemoveSpawnableVoxel(spawnableVoxels[i]);
                }
                else
                {
                    BlockBehaviour.SpawnMob(spawnableVoxels[i]);
                }
            }
        }
    }

    public void UpdateChunk()
    {

        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    if (World.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }



        World.Instance.chunksToDraw.Enqueue(this);
    }

    public bool isActive
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    public void AddActiveVoxel(VoxelState voxel)
    {
        if (!activeVoxels.Contains(voxel))
        {
            activeVoxels.Add(voxel);
        }
    }

    public void AddSpawnableVoxel(VoxelState voxel)
    {
        if (!spawnableVoxels.Contains(voxel))
        {
            spawnableVoxels.Add(voxel);
        }
    }

    public void RemoveActiveVoxel(VoxelState voxel)
    {
        for (int i = 0; i < activeVoxels.Count; i++)
        {
            if (activeVoxels[i] == voxel)
            {
                activeVoxels.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveSpawnableVoxel(VoxelState voxel)
    {
        for (int i = 0; i < spawnableVoxels.Count; i++)
        {
            if (spawnableVoxels[i] == voxel)
            {
                spawnableVoxels.RemoveAt(i);
                return;
            }
        }
    }

    void UpdateMeshData (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        VoxelState voxel = chunkData.map[x, y, z];
        if (voxel == null || voxel.neighbours == null) return;

        float rot = 0f;
        switch (voxel.orientation)
        {
            case 0:
                rot = 180f;
                break;
            case 5:
                rot = 270f;
                break;
            case 1:
                rot = 0f;
                break;
            default:
                rot = 90f;
                break;
        }

        for (int t = 0; t < 6; ++t)
        {
            int translatetT = t;

            if (voxel.orientation != 1)
            {
                if (voxel.orientation == 0)
                {
                    if (t == 0) translatetT = 1;
                    else if (t == 1) translatetT = 0;
                    else if (t == 4) translatetT = 5;
                    else if (t == 5) translatetT = 4;
                } else if (voxel.orientation == 5)
                {
                    if (t == 0) translatetT = 5;
                    else if (t == 1) translatetT = 4;
                    else if (t == 4) translatetT = 0;
                    else if (t == 5) translatetT = 1;
                }
                else if (voxel.orientation == 4)
                {
                    if (t == 0) translatetT = 4;
                    else if (t == 1) translatetT = 5;
                    else if (t == 4) translatetT = 1;
                    else if (t == 5) translatetT = 0;
                }
            }
            VoxelState neighbour = voxel.neighbours[translatetT];
            if (neighbour != null && neighbour.properties.renderNeighbourFaces && !(voxel.properties.isLiquid && chunkData.map[x, y + 1, z].properties.isLiquid))
            {
                float lightLevel = neighbour.lightAsFloat;
                int faceVertCount = 0;

                for (int i = 0; i < voxel.properties.meshData.faces[t].vertData.Length; ++i)
                {
                    VertData vertData = voxel.properties.meshData.faces[t].GetVertData(i);

                    vertices.Add(pos + vertData.GetRotatedPosition(new Vector3(0, rot, 0)));
                    normals.Add(VoxelData.faceChecks[t]);
                    colors.Add(new Color(0, 0, 0, lightLevel));

                    if (voxel.properties.isLiquid)
                    {
                        uvs.Add(voxel.properties.meshData.faces[t].vertData[i].uv);
                    }
                    else
                    {
                        AddTexture(voxel.properties.GetTextureID(t), vertData.uv);
                    }
                    faceVertCount++;
                }


                if (!voxel.properties.renderNeighbourFaces)
                {
                    for (int i = 0; i < voxel.properties.meshData.faces[t].triangles.Length; ++i)
                    {
                        triangles.Add(vertexIndex + voxel.properties.meshData.faces[t].triangles[i]);
                    }
                }
                else
                {
                    if (voxel.properties.isLiquid)
                    {
                        for (int i = 0; i < voxel.properties.meshData.faces[t].triangles.Length; ++i)
                        {
                            liquidTriangles.Add(vertexIndex + voxel.properties.meshData.faces[t].triangles[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < voxel.properties.meshData.faces[t].triangles.Length; ++i)
                        {
                            transparentTriangles.Add(vertexIndex + voxel.properties.meshData.faces[t].triangles[i]);
                        }
                    }

                }
                vertexIndex += faceVertCount;
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 3;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.SetTriangles(liquidTriangles.ToArray(), 2);
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }


    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        liquidTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newID, World.Instance._player.orientation);

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
    }

    public void EditVoxel(Vector3 pos, byte newID, int direction)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newID, direction);

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
    }



    public void UpdateSpawnableList(byte _id, Vector3Int pos)
    {
        if (_id == (byte)VoxelBlockID.AIR_BLOCK)
        {
            RemoveSpawnableVoxel(chunkData.map[pos.x, pos.y, pos.z]);

            if (pos.y == VoxelData.ChunkHeight - 1)
            {
                if (chunkData.map[pos.x, pos.y - 1, pos.z].properties.isSpawnable)
                {
                    AddSpawnableVoxel(chunkData.map[pos.x, pos.y - 1, pos.z]);
                }
            }
            else
            {
                if (pos.y > 0 && chunkData.map[pos.x, pos.y - 1, pos.z].properties.isSpawnable && chunkData.map[pos.x, pos.y + 1, pos.z].id == (byte)VoxelBlockID.AIR_BLOCK)
                {
                    AddSpawnableVoxel(chunkData.map[pos.x, pos.y - 1, pos.z]);
                }
                if (pos.y > 1 && chunkData.map[pos.x, pos.y - 2, pos.z].properties.isSpawnable && chunkData.map[pos.x, pos.y - 1, pos.z].id == (byte)VoxelBlockID.AIR_BLOCK)
                {
                    AddSpawnableVoxel(chunkData.map[pos.x, pos.y - 2, pos.z]);
                }
            }
        }
        else
        {
            if (World.Instance.blockTypes[_id].isSpawnable)
            {
                if ((pos.y == VoxelData.ChunkHeight - 1) ||
                            (pos.y == VoxelData.ChunkHeight - 2 && chunkData.map[pos.x, pos.y + 1, pos.z].id == (byte)VoxelBlockID.AIR_BLOCK) ||
                            (chunkData.map[pos.x, pos.y + 1, pos.z].id == (byte)VoxelBlockID.AIR_BLOCK && chunkData.map[pos.x, pos.y + 2, pos.z].id == (byte)VoxelBlockID.AIR_BLOCK))
                {
                    AddSpawnableVoxel(chunkData.map[pos.x, pos.y, pos.z]);
                }
            }

            if (pos.y > 0)
            {
                RemoveSpawnableVoxel(chunkData.map[pos.x, pos.y - 1, pos.z]);
            }
            if (pos.y > 1)
            {
                RemoveSpawnableVoxel(chunkData.map[pos.x, pos.y - 2, pos.z]);
            }
        }
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int t = 0; t < 6; ++t)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[t];

            if (!chunkData.IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                World.Instance.AddChunkToUpdate(World.Instance.GetChunkFromVector3(currentVoxel + position), true);
            }
        }
    }

    void AddTexture(int textureID, Vector2 uv)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;
        uvs.Add(new Vector2(x, y));
    }

}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord (int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord (Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        return other.x == x && other.z == z;
    }
}
