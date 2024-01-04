using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

public class BurstChunkData : MonoBehaviour
{
    public Vector2Int position;

    private NativeArray<float> noiseMap;
    public NativeArray<Utility.Blocks> blockMap;

    public bool finished = true;

    public WorldNoiseGenerator wng;

    public bool UseShader = false;

    private void Awake()
    {
        wng = GetComponent<WorldNoiseGenerator>();

        if (wng != null)
        {
            wng.Init();
        }
        else
        {
            Debug.LogError($"WorldNoiseGenerator at {position} is null");
        }
    }

    public void Init()
    {
        noiseMap = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z, Allocator.Persistent);
        blockMap = new NativeArray<Utility.Blocks>(Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z, Allocator.Persistent);

        PerlinNoiseJob job = new PerlinNoiseJob
        {
            position = position,
            width = Utility.CHUNK_X,
            height = Utility.CHUNK_Y,
            depth = Utility.CHUNK_Z,
            scale = WorldNoiseSettings.NOISE_SCALE,
            noiseMap = noiseMap,
            blockMap = blockMap,
            cont = wng.Continentalness,
            ero = wng.Erosion,
            pv = wng.Peaks
        };

        JobHandle jobHandle = job.Schedule(Utility.CHUNK_Y * Utility.CHUNK_X * Utility.CHUNK_Z, 64);
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
        public NativeArray<float> cont;
        public NativeArray<float> ero;
        public NativeArray<float> pv;

        void Squash(int i, int y)
        {
            int halfPoint = Mathf.FloorToInt(height * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET / 2);
            int distFromHalfPoint = Mathf.Abs(y - halfPoint);

            if (y < halfPoint)
            {
                noiseMap[i] = math.floor(noiseMap[i] + WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
            }
            else if (y > halfPoint)
            {
                noiseMap[i] = math.floor(noiseMap[i] - WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
            }
        }

        void CreateWorldShape(int i)
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

            for (int i = 0; i < WorldNoiseSettings.OCTAVES; i++)
            {
                sample += noise.pnoise(new float3(xCoord / scale, yCoord / scale, zCoord / scale) / scale2, float.MaxValue) * weight;
                weight *= WorldNoiseSettings.PERSISTENCE;
                scale2 /= WorldNoiseSettings.LACUNARITY;
            }

            int twoDIndex = x * width + z;

            float cSample = cont[twoDIndex];
            float eSample = ero[twoDIndex];
            float pvSample = pv[twoDIndex];

            sample *= (cSample + eSample + pvSample) * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET;

            noiseMap[index] = sample;

            Squash(index, y);
            CreateWorldShape(index);
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
