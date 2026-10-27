using Unity.Collections;
using Unity.Mathematics;
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
    }

    //private static void PlaceSurfaceAndSubsurfaceBlocks(NativeArray<Utility.Blocks> map, Biome biome)
    //{
    //    for (int x = 0; x < Utility.CHUNK_X; x++)
    //    {
    //        for (int z = 0; z < Utility.CHUNK_Z; z++)
    //        {

    //        }
    //    }
    //}

    //private static int FindSurface(int x, int z)
    //{
    //    int y = Utility.CHUNK_Y / 2;

    //}
}
