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
    List<Vector2> uvs = new List<Vector2>();
    Material[] materials = new Material[2];
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();
    World world;

    private bool _isActive;

    private bool isVoxelMapPopulated = false;


    public Vector3 position;

    public Chunk (ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;
        chunkObject.transform.parent = world.transform;
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "chunk_x_" + coord.x + "_z_" + coord.z;
        position = chunkObject.transform.position;

        PopulateVoxelMap();
        
    }

    void PopulateVoxelMap()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
        isVoxelMapPopulated = true;
        lock (world.chunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }
        if (world.settings.enableAnimatedChunks)
        {
            chunkObject.AddComponent<ChunkLoadAnimation>();
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        return true;
    }

    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return world.GetVoxelState(pos + position);
        }

        return voxelMap[x, y, z];
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }


    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod vMod = modifications.Dequeue();
            Vector3 pos = vMod.position -= position;
            if (pos.y >= VoxelData.ChunkHeight) continue;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = vMod.id;
        }

        ClearMeshData();
        CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        lock (world.chunksToDraw)
        {
            world.chunksToDraw.Enqueue(this);
        }
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

    public bool isEditable
    {
        get
        {
            if (!isVoxelMapPopulated) return false;
            else return true;
        }
    }

    void UpdateMeshData (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        byte blockID = voxelMap[x, y, z].id;
        bool isTransparent = world.blockTypes[blockID].renderNeighbourFaces;

        for (int t = 0; t < 6; ++t)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[t]);

            if (neighbor != null && world.blockTypes[neighbor.id].renderNeighbourFaces)
            {

                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[t, 0]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[t, 1]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[t, 2]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[t, 3]]);

                normals.Add(VoxelData.faceChecks[t]);
                normals.Add(VoxelData.faceChecks[t]);
                normals.Add(VoxelData.faceChecks[t]);
                normals.Add(VoxelData.faceChecks[t]);

                AddTexture(world.blockTypes[blockID].GetTextureID(t));

                float lightLevel = neighbor.globalLightPercent;

                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));

                if (!world.blockTypes[neighbor.id].renderNeighbourFaces)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }
                vertexIndex += 4;

            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }

    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; ++x)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; ++z)
            {
                float lightRay = 1f;
                for (int y = VoxelData.ChunkHeight - 1; y >= 0; --y)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];
                    if (thisVoxel.id > 0 && world.blockTypes[thisVoxel.id].transparency < lightRay)
                    {
                        lightRay = world.blockTypes[thisVoxel.id].transparency;
                    }
                    thisVoxel.globalLightPercent = lightRay;
                    voxelMap[x, y, z] = thisVoxel;

                    if (lightRay > VoxelData.lightFalloff)
                    {
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                    }
                }

            }
        }

        while (litVoxels.Count > 0)
        {
            Vector3Int lV = litVoxels.Dequeue();
            for (int t = 0; t < 6; ++t)
            {
                Vector3 currentVoxel = lV + VoxelData.faceChecks[t];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent < voxelMap[lV.x, lV.y, lV.z].globalLightPercent - VoxelData.lightFalloff)
                    {
                        voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = voxelMap[lV.x, lV.y, lV.z].globalLightPercent - VoxelData.lightFalloff;

                        if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent > VoxelData.lightFalloff)
                        {
                            litVoxels.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
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

        voxelMap[xCheck, yCheck, zCheck].id = newID;
        lock (world.chunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int t = 0; t < 6; ++t)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[t];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + position));
            }
        }
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
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

public class VoxelState
{
    public byte id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }

    public VoxelState(byte _id)
    {
        id = _id;
    }
}
