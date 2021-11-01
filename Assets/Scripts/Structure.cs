using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 position, int minTreeHeight, int maxTreeHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(position, minTreeHeight, maxTreeHeight);
            case 1:
                return MakeCacti(position, minTreeHeight, maxTreeHeight);
            default:
                return new Queue<VoxelMod>();
        }
    }

    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTreeHeight, int maxTreeHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTreeHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 1200f, 3f));

        if (height < minTreeHeight) height = minTreeHeight;

        for (int i = 1; i < height; ++i)
        {
            queue.Enqueue(new VoxelMod(new Vector3(0, i, 0) + position, 7));
        }

        for (int y = 0; y < 7; ++y)
        {
            int cutValue = Mathf.Abs(7 / 2 - y) / 2;
            for (int x = -3 + cutValue; x < 4 - cutValue; ++x)
            {
                for (int z = -3 + cutValue; z < 4 - cutValue; ++z)
                {
                    if (y < 2 && x == 0 && z == 0) continue;
                    queue.Enqueue(new VoxelMod(new Vector3(x, height + y - 2, z) + position, 11));
                }   
            }
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeCacti(Vector3 position, int minTreeHeight, int maxTreeHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTreeHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 3321f, 4f));

        if (height < minTreeHeight) height = minTreeHeight;

        for (int i = 1; i <= height; ++i)
        {
            queue.Enqueue(new VoxelMod(new Vector3(0, i, 0) + position, 12));
        }

        return queue;
    }
}
