//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Burst;
//using UnityEngine;


//public class ChunkData
//{
//    private NativeArray<float> NoiseMap;
//    public Utility.Blocks[] Blocks { get; private set; }


//    private Vector2Int _position;

//    public ChunkData(Vector2Int position)
//    {
//        NoiseMap = new NativeArray<float>(World.instance.chunkDimensions.x * World.instance.chunkDimensions.y * World.instance.chunkDimensions.z, Allocator.Persistent);

//        _position = position;
//    }

//    void Squash(int i, int y)
//    {
//        int halfPoint = Mathf.FloorToInt(World.instance.chunkDimensions.y * Utility.DEFAULT_HEIGHT_OFFSET / 2);
//        int distFromHalfPoint = Mathf.Abs(y - halfPoint);

//        if (y < halfPoint)
//        {
//            NoiseMap[i] = Mathf.FloorToInt(NoiseMap[i] + Utility.SQUASH_FACTOR * distFromHalfPoint);
//        }
//        else if (y > halfPoint)
//        {
//            NoiseMap[i] = Mathf.FloorToInt(NoiseMap[i] - Utility.SQUASH_FACTOR * distFromHalfPoint);
//        }
//    }
    

//    public void GenerateHeigthMap()
//    {
//        NoiseMap = new int[World.instance.chunkDimensions.x * World.instance.chunkDimensions.y * World.instance.chunkDimensions.z];

//        for (int x = 0; x < World.instance.chunkDimensions.x; x++)
//        {
//            for (int y = 0; y < World.instance.chunkDimensions.y; y++)
//            {
//                for (int z = 0; z < World.instance.chunkDimensions.z; z++)
//                {
//                    int i = GetBlockIndex(x, y, z);

//                    float xf = x + _position.x + 0.001f;
//                    float yf = y + 0.001f;
//                    float zf = z + _position.y;

//                    NoiseMap[i] = Mathf.FloorToInt(Perlin.Noise(xf * Utility.NOISE_SCALE, yf * Utility.NOISE_SCALE, zf * Utility.NOISE_SCALE, Utility.OCTAVES) * World.instance.chunkDimensions.y * Utility.DEFAULT_HEIGHT_OFFSET);

//                    Squash(i, y);
//                }
//            }
//        }
//    }


//    public void GenerateBlocks()
//    {
//        Blocks = new Utility.Blocks[World.instance.chunkDimensions.x * World.instance.chunkDimensions.y * World.instance.chunkDimensions.z];

//        for (int x = 0; x < World.instance.chunkDimensions.x; x++)
//        {
//            for (int y = 0; y < World.instance.chunkDimensions.y; y++)
//            {
//                for (int z = 0; z < World.instance.chunkDimensions.z; z++)
//                {
//                    int i = GetBlockIndex(x, y, z);

//                    if (NoiseMap[i] < 0)
//                    {
//                        Blocks[i] = Utility.Blocks.Air;
//                    }
//                    else
//                    {
//                        Blocks[i] = Utility.Blocks.Stone;
//                    }
//                }
//            }
//        }
//    }

//    public int GetBlockIndex(int x, int y, int z)
//    {
//        return z * World.instance.chunkDimensions.x * World.instance.chunkDimensions.y + y * World.instance.chunkDimensions.x + x;
//    }

//    public Utility.Blocks GetBlock(Vector3Int index)
//    {
//        if (index.x >= World.instance.chunkDimensions.x || index.y >= World.instance.chunkDimensions.y || index.z >= World.instance.chunkDimensions.z || index.x < 0 || index.y < 0 || index.z < 0)
//            return Utility.Blocks.Air;
//        return Blocks[GetBlockIndex(index.x, index.y, index.z)];
//    }

//    [BurstCompile]


////}