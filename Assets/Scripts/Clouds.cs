using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int cloudHeight = VoxelData.ChunkHeight;
    public int cloudDepth = 4;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMat = null;
    [SerializeField] private World world = null;
    bool[,] cloudData;

    
    int cloudTextWidth;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> cloudsDict = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        cloudHeight = VoxelData.ChunkHeight;
        cloudTextWidth = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-cloudTextWidth / 2, 0, -cloudTextWidth / 2);

        transform.position = new Vector3(VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);

        LoadCloudData();
        CreateClouds();
    }

    private void LoadCloudData()
    {
        cloudData = new bool[cloudTextWidth, cloudTextWidth];
        Color[] cloudTex = cloudPattern.GetPixels();
        for (int x = 0; x < cloudTextWidth; ++x)
        {
            for (int y = 0; y < cloudTextWidth; ++y)
            {
                cloudData[x, y] = (cloudTex[y * cloudTextWidth + x].a > 0);
            }
        }

    }

    private void CreateClouds()
    {
        if (world.settings.clouds == CloudStyle.Off) return;

        for (int x = 0; x < cloudTextWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTextWidth; y += cloudTileSize)
            {
                Mesh cloudMesh;
                if (world.settings.clouds == CloudStyle.Fast)
                    cloudMesh = CreateFastCloudMesh(x, y);
                else
                    cloudMesh = CreateFancyCloudMesh(x, y);

                Vector3 position = new Vector3(x, 0, y);
                position += transform.position + offset;
                position.y = cloudHeight;
                cloudsDict.Add(CloudTilePosFromVector3(position), CreateCloudTile(cloudMesh, position));

            }
        }
    }

    private Mesh CreateFastCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int _x = 0; _x < cloudTileSize; ++_x)
        {
            for (int _z = 0; _z < cloudTileSize; ++_z)
            {
                int xVal = x + _x;
                int zVal = z + _z;

                if (cloudData[xVal, zVal])
                {
                    vertices.Add(new Vector3(_x, 0, _z));
                    vertices.Add(new Vector3(_x, 0, _z + 1));
                    vertices.Add(new Vector3(_x + 1, 0, _z + 1));
                    vertices.Add(new Vector3(_x + 1, 0, _z));

                    normals.Add(Vector3.down);
                    normals.Add(Vector3.down);
                    normals.Add(Vector3.down);
                    normals.Add(Vector3.down);

                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);

                    vertCount += 4;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;

    }

    private Mesh CreateFancyCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int _x = 0; _x < cloudTileSize; ++_x)
        {
            for (int _z = 0; _z < cloudTileSize; ++_z)
            {
                int xVal = x + _x;
                int zVal = z + _z;

                if (cloudData[xVal, zVal])
                {
                    for (int t = 0; t < 6; ++t)
                    {
                        if (!CheckCloudData(new Vector3Int(xVal, 0, zVal) + VoxelData.faceChecks[t]))
                        {
                            for (int i = 0; i < 4; ++i)
                            {
                                Vector3 vertex = new Vector3Int(_x, 0, _z);
                                vertex += VoxelData.voxelVertices[VoxelData.voxelTriangles[t, i]];
                                vertex.y *= cloudDepth;
                                vertices.Add(vertex);
                            }

                            for (int i = 0; i < 4; ++i)
                            {
                                normals.Add(VoxelData.faceChecks[t]);
                            }

                            triangles.Add(vertCount);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 3);

                            vertCount += 4;
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private bool CheckCloudData(Vector3Int point)
    {
        if (point.y != 0) return false;

        int x = point.x;
        int z = point.z;

        if (x < 0) x = cloudTextWidth - 1;
        if (z < 0) z = cloudTextWidth - 1;
        if (x > cloudTextWidth - 1) x = 0;
        if (z > cloudTextWidth - 1) z = 0;

        return cloudData[x, z];
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        GameObject newCloudTile = new GameObject();
        newCloudTile.transform.position = position;
        newCloudTile.transform.parent = transform;
        newCloudTile.name = "cloud_x_" + position.x + "_z_" + position.z;
        MeshFilter meshFilter = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newCloudTile.AddComponent<MeshRenderer>();
        meshRenderer.material = cloudMat;
        meshFilter.mesh = mesh;

        return newCloudTile;
    }

    private Vector2Int CloudTilePosFromVector3(Vector3 pos)
    {
        return new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));
    }

    private int CloudTileCoordFromFloat(float value)
    {
        float weight = value / (float)cloudTextWidth;
        weight -= Mathf.FloorToInt(weight);

        return Mathf.FloorToInt((float)cloudTextWidth * weight);
    }

    public void UpdateClouds()
    {
        if (world.settings.clouds == CloudStyle.Off) return;
        for (int x = 0; x < cloudTextWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTextWidth; y += cloudTileSize)
            {
                Vector3 position = world.player.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                Vector2Int cloudPosition = CloudTilePosFromVector3(position);

                cloudsDict[cloudPosition].transform.position = position;
            }
        }
    }

    private int RoundToCloud(float value)
    {
        return Mathf.FloorToInt(value / cloudTileSize) * cloudTileSize;
    }
}

public enum CloudStyle
{
    Off,
    Fast,
    Fancy
}
