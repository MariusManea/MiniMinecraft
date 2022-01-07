using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;
    float timer;
    float frameRate;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {
        world = FindObjectOfType<World>();
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    void Update()
    {
        string debugText = "Mini Minecraft\n";
        debugText += frameRate + " FPS\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + Mathf.FloorToInt(world.player.transform.position.y) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks) + "\n";

        string direction = "";
        switch (world._player.orientation)
        {
            case 1:
                direction = "South";
                break;
            case 5:
                direction = "East";
                break;
            case 0:
                direction = "North";
                break;
            default:
                direction = "West";
                break;
        }
        debugText += "\n" + "Facing Direction: " + direction + "\n";
        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        } 
        else
        {
            timer += Time.deltaTime;
        }

    }
}
