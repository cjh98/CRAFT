using UnityEngine;
using Unity.Collections;

public class WorldPopulator
{
    public static void PopulateWorld(NativeArray<Utility.Blocks> map)
    {
        for (int i = 0; i < map.Length; i++)
        {
            SurfaceBlocks(i, map);
            SubsurfaceBlocks(i, map);
        }
    }

    private static void SurfaceBlocks(int i, NativeArray<Utility.Blocks> map)
    {
        int upY = i + Utility.CHUNK_X;
        int downY = i - Utility.CHUNK_X;

        // place grass
        if (downY > 0 && upY < map.Length)
        {
            if (map[upY] == Utility.Blocks.Air && map[downY] == Utility.Blocks.Stone)
            {
                map[i] = Utility.Blocks.Grass;
            }
        }
    }

    private static void SubsurfaceBlocks(int i, NativeArray<Utility.Blocks> map)
    {
        // place dirt
        if (map[i] == Utility.Blocks.Grass)
        {
            int down1 = i - Utility.CHUNK_X;
            int down2 = i - Utility.CHUNK_X * 2;
            int down3 = i - Utility.CHUNK_X * 3;

            if (down1 > 0)
            {
                map[down1] = Utility.Blocks.Dirt;
            }

            if (down2 > 0)
            {
                map[down2] = Utility.Blocks.Dirt;
            }

            if (down3 > 0)
            {
                map[down3] = Utility.Blocks.Dirt;
            }
        }
    }
}
