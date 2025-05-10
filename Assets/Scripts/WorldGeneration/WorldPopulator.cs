using Unity.Collections;
using UnityEngine;

public class WorldPopulator
{
    private static readonly Biome DefaultBiome = new Biome
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

        int chunkSize = Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z;
        for (int i = 0; i < chunkSize; i++)
        {
            int y = (i / Utility.CHUNK_X) % Utility.CHUNK_Y; // Extract Y coordinate
            if (y < Utility.CHUNK_Y - 1)
            {
                PlaceSurfaceAndSubsurfaceBlocks(i, chunkData.BlockMap, DefaultBiome);
            }
        }
    }

    private static void PlaceSurfaceAndSubsurfaceBlocks(int i, NativeArray<Utility.Blocks> map, Biome biome)
    {
        int chunkWidth = Utility.CHUNK_X;
        int chunkHeight = Utility.CHUNK_Y;

        //int x = i % chunkWidth;
        int y = (i / chunkWidth) % chunkHeight;
        //int z = i / (chunkWidth * chunkHeight);

        int upY = i + chunkWidth;
        int downY = i - chunkWidth;

        if (y < chunkHeight - 1 && y > 0)
        {
            // Place surface block
            if (map[upY] == Utility.Blocks.Air && map[downY] == Utility.Blocks.Stone)
            {
                map[i] = biome.surfaceBlock;

                // Place subsurface blocks
                for (int d = 1; d <= 3; d++)
                {
                    int downIndex = i - d * chunkWidth;
                    if (downIndex >= 0)
                    {
                        map[downIndex] = biome.subSurfaceBlock;
                    }
                }
            }
        }
    }
}
