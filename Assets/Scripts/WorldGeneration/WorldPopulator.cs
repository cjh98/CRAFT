using Unity.Collections;
using UnityEngine;

public class WorldPopulator
{
    private static readonly Biome DefaultBiome = new()
    {
        name = "default",
        surfaceBlock = Utility.Blocks.Grass,
        subSurfaceBlock = Utility.Blocks.Dirt,
        squashFactor = 1.0f
    };

    public static void PopulateWorld(BurstChunkData chunkData)
    {
        if (chunkData == null)
        {
            Debug.LogError("chunkData is null");
            return;
        }

        for (int i = 0; i < Utility.CHUNK_TOTAL_BLOCKS; i++)
        {
            int y = (i / Utility.CHUNK_X) % Utility.CHUNK_Y; // Extract Y coordinate
            // array bounds check
            if (y < Utility.CHUNK_Y - 1)
            {
                PlaceSurfaceAndSubsurfaceBlocks(i, chunkData.BlockMap, DefaultBiome);
            }
        }
    }

    private static void PlaceSurfaceAndSubsurfaceBlocks(int i, NativeArray<Utility.Blocks> map, Biome biome)
    {
        //int x = i % chunkWidth;
        //int z = i / (chunkWidth * chunkHeight);

        int y = (i / Utility.CHUNK_X) % Utility.CHUNK_Y;

        int upY = i + Utility.CHUNK_X;
        int downY = i - Utility.CHUNK_X;

        if (y < Utility.CHUNK_Y - 1 && y > 0)
        {
            // Place surface block
            if (map[upY] == Utility.Blocks.Air && map[downY] == Utility.Blocks.Stone)
            {
                map[i] = biome.surfaceBlock;

                // Place subsurface blocks
                for (int d = 1; d <= 3; d++)
                {
                    int downIndex = i - d * Utility.CHUNK_X;
                    if (downIndex >= 0)
                    {
                        map[downIndex] = biome.subSurfaceBlock;
                    }
                }
            }
        }
    }
}
