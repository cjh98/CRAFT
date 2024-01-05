using Unity.Collections;
using UnityEngine;

public class WorldPopulator
{
    public static void PopulateWorld(BurstChunkData chunkData)
    {
        if (chunkData == null)
        {
            Debug.LogError("chunkData is null");
            return;
        }

        //Debug.Log(string.Join(", ", chunkData.wng.Continentalness));

        for (int i = 0; i < chunkData.BlockMap.Length; i++)
        {
            Biome biome = DetermineBlockBiome(i, chunkData);

            SurfaceBlocks(i, chunkData.BlockMap, biome);
            SubsurfaceBlocks(i, chunkData.BlockMap, biome);
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

    public static Biome DetermineBlockBiome(int i, BurstChunkData data)
    {
        int z = i / (Utility.CHUNK_X * Utility.CHUNK_Y);
        int y = i % (Utility.CHUNK_X * Utility.CHUNK_Y) / Utility.CHUNK_X;  
        int x = i % (Utility.CHUNK_X * Utility.CHUNK_Y) % Utility.CHUNK_X;  

        int index2D = x + z * Utility.CHUNK_X;

        float continentalness = data.wng.Continentalness[index2D];

        if (continentalness < 0)
        {
            return Biomes.instance.biomes[1];
        }
        else
        {
            return Biomes.instance.biomes[0];
        }
    }
}