using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTreeHeight, int maxTreeHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTreeHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 1200f, 3f));

        if (height < minTreeHeight) height = minTreeHeight;

        for (int i = 1; i < height; ++i)
        {
            queue.Enqueue(new VoxelMod(new Vector3(0, i, 0) + position, 7));
        }

        for (int x = -3; x < 4; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                for (int z = -3; z < 4; ++z)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(x, height + y, z) + position, 11));

                }
            }
        }

        return queue;
    }
}
