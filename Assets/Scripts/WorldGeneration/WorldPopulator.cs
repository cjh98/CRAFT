using UnityEngine;
using Unity.Mathematics;

public class WorldPopulator
{
    public static void PopulateWorld(ChunkShaderData chunkData)
    {
        if (chunkData == null)
        {
            Debug.LogError("chunkData is null");
            return;
        }

        for (int i = 0; i < chunkData.BlockMap.Length; i++)
        {
            //Biome biome = DetermineChunkBiome(chunkData);
            Biome biome;

            biome.name = "Test";
            biome.surfaceBlock = Utility.Blocks.Sand;
            biome.subSurfaceBlock = Utility.Blocks.Sand;

            SurfaceBlocks(i, chunkData.BlockMap, biome);
            SubsurfaceBlocks(i, chunkData.BlockMap, biome);
        }
    }

    private static void SurfaceBlocks(int i, Utility.Blocks[] map, Biome biome)
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

    private static void SubsurfaceBlocks(int i, Utility.Blocks[] map, Biome biome)
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

    //public static float GenerateChunkMoistureValue(Vector2Int chunkPos)
    //{
    //    float x = chunkPos.x + 0.001f;
    //    float y = chunkPos.y + 0.001f;

    //    return noise.pnoise(new float2(x / WorldNoiseSettings.BIOME_SCALE, y / WorldNoiseSettings.BIOME_SCALE), float.MaxValue);
    //}

    //private static Biome DetermineBlockBiome(BurstChunkData data)
    //{
    //    // desert
    //    if (moisture < 0)
    //    {
    //        return Biomes.instance.biomes[1];
    //    }
    //    else
    //    {
    //        return Biomes.instance.biomes[0];
    //    }
    //}
}