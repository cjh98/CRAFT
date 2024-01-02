using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

public class BurstChunkData : MonoBehaviour
{
    public Vector2Int position;
    private int width = Utility.CHUNK_X;
    private int height = Utility.CHUNK_Y;
    private int depth = Utility.CHUNK_Z;
    private float scale = Utility.NOISE_SCALE;

    private NativeArray<float> noiseMap;
    public NativeArray<Utility.Blocks> blockMap;

    public bool finished = false;

    public void Init()
    {
        noiseMap = new NativeArray<float>(width * height * depth, Allocator.Persistent);
        blockMap = new NativeArray<Utility.Blocks>(width * height * depth, Allocator.Persistent);

        PerlinNoiseJob job = new PerlinNoiseJob
        {
            position = position,
            width = width,
            height = height,
            depth = depth,
            scale = scale,
            noiseMap = noiseMap,
            blockMap = blockMap
        };

        JobHandle jobHandle = job.Schedule(height * width * depth, 64);
        jobHandle.Complete();
    }

    void OnDestroy()
    {
        noiseMap.Dispose();
        blockMap.Dispose();
    }

    [BurstCompile]
    struct PerlinNoiseJob : IJobParallelFor
    {
        public Vector2Int position;
        public int width;
        public int height;
        public int depth;
        public float scale;
        public NativeArray<float> noiseMap;
        public NativeArray<Utility.Blocks> blockMap;

        void Squash(int i, int y)
        {
            int halfPoint = Mathf.FloorToInt(height * Utility.DEFAULT_HEIGHT_OFFSET / 2);
            int distFromHalfPoint = Mathf.Abs(y - halfPoint);

            if (y < halfPoint)
            {
                noiseMap[i] = math.floor(noiseMap[i] + Utility.SQUASH_FACTOR * distFromHalfPoint);
            }
            else if (y > halfPoint)
            {
                noiseMap[i] = math.floor(noiseMap[i] - Utility.SQUASH_FACTOR * distFromHalfPoint);
            }
        }

        void CreateWorld(int i)
        {
            // initial pass: solid vs air
            if (noiseMap[i] < 0)
            {
                blockMap[i] = Utility.Blocks.Air;
            }
            else
            {
                blockMap[i] = Utility.Blocks.Stone;
            }

            // grass
            if (noiseMap[i] >= 0 && noiseMap[i] < 30)
            {
                blockMap[i] = Utility.Blocks.Grass; 
            }

            // dirt
            if (noiseMap[i] >= 30 && noiseMap[i] < 60)
            {
                blockMap[i] = Utility.Blocks.Dirt;
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

            for (int i = 0; i < Utility.OCTAVES; i++)
            {
                sample += noise.pnoise(new float3(xCoord / scale, yCoord / scale, zCoord / scale) / scale2, float.MaxValue) * weight;
                weight *= Utility.PERSISTENCE;
                scale2 /= Utility.LACUNARITY;
            }
            
            sample *= height * Utility.DEFAULT_HEIGHT_OFFSET;

            noiseMap[index] = sample;

            Squash(index, y);
            CreateWorld(index);
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
        return blockMap[GetBlockIndex(index.x, index.y, index.z)];
    }
}
