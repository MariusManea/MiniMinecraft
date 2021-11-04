using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public static class SaveSystem
{
    public static void SaveWorld (WorldData world)
    {
        string savePath = World.Instance.appPath + "/worlds/" + world.worldName + "/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);
        formatter.Serialize(stream, world);

        stream.Close();

        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();
    }

    public static void SaveChunks(WorldData world)
    {
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();

        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;
        }
        Debug.Log("Saved " + count + " chunks");
    }


    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = "_chunk_x_" + chunk.position.x + "_z_" + chunk.position.y + ".chunk";

        string savePath = World.Instance.appPath + "/worlds/" + worldName + "/chunks/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + chunkName, FileMode.Create);
        formatter.Serialize(stream, chunk);

        stream.Close();
    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = World.Instance.appPath + "/worlds/" + worldName + "/";
        if (File.Exists(loadPath + "world.world"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);

            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();
            return new WorldData(world);
        }
        else
        {
            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);
            return world;
        }
    }

    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = "_chunk_x_" + position.x + "_z_" + position.y + ".chunk";


        string loadPath = World.Instance.appPath + "/worlds/" + worldName + "/chunks/" + chunkName;
        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();
            return chunkData;
        }
        return null;
    }
}
