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

        position = new Vector2Int(Mathf.FloorToInt(transform.position.x / Utility.CHUNK_X), Mathf.FloorToInt(transform.position.z / Utility.CHUNK_Z));
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

        private void Squash(int i, int y)
        {
            int halfPoint = Mathf.FloorToInt(Utility.CHUNK_Y * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET / 2);
            int distFromHalfPoint = Mathf.Abs(y - halfPoint);

            if (y < halfPoint)
            {
                densityMap[i] = math.floor(densityMap[i] + WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
            }
            else if (y > halfPoint)
            {
                densityMap[i] = math.floor(densityMap[i] - WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
            }
        }

        private void CreateWorldShape(int i)
        {
            if (densityMap[i] < 0)
            {
                blockMap[i] = Utility.Blocks.Air;
            }
            else
            {
                blockMap[i] = Utility.Blocks.Stone;
            }
        }

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

            Squash(index, y);
            CreateWorldShape(index);
        }
    }


    public int GetBlockIndex(int x, int y, int z)
    {
        return z * Utility.CHUNK_X * Utility.CHUNK_Y + y * Utility.CHUNK_X + x;
    }

    public Utility.Blocks GetBlock(Vector3Int index)
    {
        if (index.x >= Utility.CHUNK_X || index.y >= Utility.CHUNK_Y || index.z >= Utility.CHUNK_Z || index.x < 0 || index.y < 0 || index.z < 0)
            return Utility.Blocks.Air;
        return BlockMap[GetBlockIndex(index.x, index.y, index.z)];
    }
}
