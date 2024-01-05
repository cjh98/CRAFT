using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

public class BurstChunkData : MonoBehaviour
{
    public Vector2Int position;

    private NativeArray<float> DensityMap;
    public NativeArray<Utility.Blocks> BlockMap;

    public bool finished = true;

    public WorldNoiseGenerator wng;

    public void Init()
    {
        DensityMap = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z, Allocator.Persistent);
        BlockMap = new NativeArray<Utility.Blocks>(Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z, Allocator.Persistent);

        PerlinNoiseJob job = new PerlinNoiseJob
        {
            position = position,
            width = Utility.CHUNK_X,
            height = Utility.CHUNK_Y,
            depth = Utility.CHUNK_Z,
            scale = WorldNoiseSettings.NOISE_SCALE,
            densityMap = DensityMap,
            blockMap = BlockMap
        };

        JobHandle jobHandle = job.Schedule(Utility.CHUNK_Y * Utility.CHUNK_X * Utility.CHUNK_Z, 64);
        jobHandle.Complete();

        wng = GetComponent<WorldNoiseGenerator>();
        position = new Vector2Int(Mathf.FloorToInt(transform.position.x / Utility.CHUNK_X), Mathf.FloorToInt(transform.position.z / Utility.CHUNK_Z));

        if (wng != null)
        {
            wng.Position = position;
            wng.Init();
        }
        else
        {
            Debug.LogError($"WorldNoiseGenerator at {position} is null");
            return;
        }

        FinalizeDensityMapWithWorldNoise();
    }

    void OnDestroy()
    {
        DensityMap.Dispose();
        BlockMap.Dispose();
    }

    [BurstCompile]
    struct PerlinNoiseJob : IJobParallelFor
    {
        public Vector2Int position;
        public int width;
        public int height;
        public int depth;
        public float scale;
        public NativeArray<float> densityMap;
        public NativeArray<Utility.Blocks> blockMap;

        public void Execute(int index)
        {
            int z = index / (width * height);
            int y = index % (width * height) / width;
            int x = index % (width * height) % width;

            float xCoord = ((float)x + position.x);
            float yCoord = y;
            float zCoord = ((float)z + position.y);

            float sample = 0.0f;
            float weight = 1.0f;
            float scale2 = 1.0f;

            int octaves = WorldNoiseSettings.OCTAVES;
            for (int i = 0; i < octaves - 1; i += 2)
            {
                float3 coord1 = new float3(xCoord / scale, yCoord / scale, zCoord / scale) / scale2;
                float3 coord2 = new float3(xCoord / scale, yCoord / scale, zCoord / scale) / scale2;

                sample += noise.pnoise(coord1, float.MaxValue) * weight;
                sample += noise.pnoise(coord2, float.MaxValue) * weight;
                
                weight *= WorldNoiseSettings.PERSISTENCE * WorldNoiseSettings.PERSISTENCE;
                scale2 *= WorldNoiseSettings.LACUNARITY * WorldNoiseSettings.LACUNARITY;
            }

            // Handle the last iteration if OCTAVES is an odd number
            if (octaves % 2 == 1)
            {
                float3 coord = new float3(xCoord / scale, yCoord / scale, zCoord / scale) / scale2;
                sample += noise.pnoise(coord, float.MaxValue) * weight;
            }

            densityMap[index] = sample * Utility.CHUNK_Y * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET;
        }
    }

    private void FinalizeDensityMapWithWorldNoise()
    {
        for (int z = 0; z < Utility.CHUNK_Z; z++)
        {
            for (int y = 0; y < Utility.CHUNK_Y; y++)
            {
                for (int x = 0; x < Utility.CHUNK_X; x++)
                {
                    int index2D = x + z;
                    int index3D = GetBlockIndex(x, y, z);

                    //print(index3D);

                    float continentalness = wng.Continentalness[index2D];
                    //float erosion =         wng.Erosion[index2D];
                    //float peaks =           wng.Peaks[index2D];

                    float continentalnessPower = WorldNoiseSettings.Instance.ContinentalnessCurve.Evaluate(continentalness);

                    float total = continentalnessPower; //+ erosion + peaks;

                    //DensityMap[index3D] += total;

                    float lerpFactor = 0.5f;

                    DensityMap[index3D] = Mathf.Lerp(DensityMap[index3D], total, lerpFactor);



                    //print(string.Join(", ", DensityMap[index3D]));

                    Squash(index3D, y);
                    CreateWorldShape(index3D);
                }
            }
        }
    }

    private void Squash(int i, int y)
    {
        int halfPoint = Mathf.FloorToInt(Utility.CHUNK_Y * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET / 2);
        int distFromHalfPoint = Mathf.Abs(y - halfPoint);

        Biome b = WorldPopulator.DetermineBlockBiome(i, this);

        float squashValue = math.floor(b.squashFactor * distFromHalfPoint);

        if (y < halfPoint)
        {
            DensityMap[i] = Mathf.Lerp(DensityMap[i], squashValue, 0.5f);
        }
        else if (y > halfPoint)
        {
            DensityMap[i] = Mathf.Lerp(DensityMap[i], -squashValue, 0.5f);
        }
    }

    private void CreateWorldShape(int i)
    {
        if (DensityMap[i] < 0)
        {
            BlockMap[i] = Utility.Blocks.Air;
        }
        else
        {
            BlockMap[i] = Utility.Blocks.Stone;
        }
    }

    public int GetBlockIndex(int x, int y, int z)
    {
        return z * World.instance.chunkDimensions.x * World.instance.chunkDimensions.y + y * World.instance.chunkDimensions.x + x;
    }

    public Utility.Blocks GetBlock(Vector3Int index)
    {
        if (index.x >= World.instance.chunkDimensions.x || index.y >= World.instance.chunkDimensions.y || index.z >= World.instance.chunkDimensions.z || index.x < 0 || index.y < 0 || index.z < 0)
            return Utility.Blocks.Air;
        return BlockMap[GetBlockIndex(index.x, index.y, index.z)];
    }
}
