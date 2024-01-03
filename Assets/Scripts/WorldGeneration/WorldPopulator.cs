using UnityEngine;
using Unity.Collections;

public class WorldPopulator
{
    public static void PopulateWorld(NativeArray<Utility.Blocks> map, Biome biome)
    {
        for (int i = 0; i < map.Length; i++)
        {
            SurfaceBlocks(i, map, biome);
            SubsurfaceBlocks(i, map, biome);
        }
    }

    private static void SurfaceBlocks(int i, NativeArray<Utility.Blocks> map, Biome biome)
    {
        int upY = i + Utility.CHUNK_X;
        int downY = i - Utility.CHUNK_X;

        // place surface block like grass, sand etc
        if (downY > 0 && upY < map.Length)
        {
            if (map[upY] == Utility.Blocks.Air && map[downY] == Utility.Blocks.Stone)
            {
                map[i] = biome.surfaceBlock;
            }
        }
    }

    private static void SubsurfaceBlocks(int i, NativeArray<Utility.Blocks> map, Biome biome)
    {
        // place subsurface block like dirt, sandstone etc
        if (map[i] == biome.surfaceBlock)
        {
            int down1 = i - Utility.CHUNK_X;
            int down2 = i - Utility.CHUNK_X * 2;
            int down3 = i - Utility.CHUNK_X * 3;

            if (down1 > 0)
            {
                map[down1] = biome.subSurfaceBlock;
            }

            if (down2 > 0)
            {
                map[down2] = biome.subSurfaceBlock;
            }

            if (down3 > 0)
            {
                map[down3] = biome.subSurfaceBlock;
            }
        }
    }
}

public struct Biome
{
    public Utility.Blocks surfaceBlock;
    public Utility.Blocks subSurfaceBlock;
}