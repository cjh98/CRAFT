using UnityEngine;
using Unity.Mathematics;

public class InterpolateChunkData : MonoBehaviour
{
    public Vector2Int chunkPosition;

    [System.NonSerialized]
    public float[] noiseMap;
    [System.NonSerialized]
    public Utility.Blocks[] blockMap;

    private readonly int xStep = 4;
    private readonly int yStep = 2;
    private readonly int zStep = 4;

    public bool finished = true;

    public void Init()
    {
        int interpX = Utility.CHUNK_X / xStep;
        int interpY = Utility.CHUNK_Y / yStep;
        int interpZ = Utility.CHUNK_Z / zStep;

        int interpSize = interpX * interpY * interpZ;
        int totalSize = Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z;

        noiseMap = new float[interpSize];
        blockMap = new Utility.Blocks[totalSize];

        for (int z = 0; z < interpZ; z++)
        {
            for (int y = 0; y < interpY; y++)
            {
                for (int x = 0; x < interpX; x++)
                {
                    float xf = x * xStep + 0.001f + chunkPosition.x;
                    float yf = y * yStep + 0.001f;
                    float zf = z * zStep + 0.001f + chunkPosition.y;

                    float sample = noise.pnoise(new float3(xf / Utility.NOISE_SCALE, yf / Utility.NOISE_SCALE, zf / Utility.NOISE_SCALE), float.MaxValue);
                    sample *= Utility.CHUNK_Y * Utility.DEFAULT_HEIGHT_OFFSET;

                    int i = z * (interpX) * (interpY) + y * (interpX) + x;

                    noiseMap[i] = sample;

                    Squash(i);
                }
            }
        }

        for (int z = 0; z < Utility.CHUNK_Z; z++)
        {
            for (int y = 0; y < Utility.CHUNK_Y; y++)
            {
                for (int x = 0; x < Utility.CHUNK_X; x++)
                {
                    int i = z * (Utility.CHUNK_X) * (Utility.CHUNK_Y) + y * (Utility.CHUNK_X) + x;

                    CreateWorldShape(i, new Vector3(x, y, z));
                }
            }
        }
    }

    void Squash(int i)
    {
        int y = i % ((Utility.CHUNK_X / xStep) * (Utility.CHUNK_Y / yStep)) / (Utility.CHUNK_Z / zStep);

        int halfPoint = Mathf.FloorToInt(Utility.CHUNK_Y * Utility.DEFAULT_HEIGHT_OFFSET / 2);
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

    void CreateWorldShape(int i, Vector3 pos)
    {
        float noiseValue = GetInterpolatedNoiseValue(pos);

        // initial pass: solid vs air
        if (noiseValue < 0)
        {
            blockMap[i] = Utility.Blocks.Air;
        }
        else
        {
            blockMap[i] = Utility.Blocks.Stone;
        }
    }

    public float GetInterpolatedNoiseValue(Vector3 pos)
    {
        // Convert the world position to local position within the chunk
        Vector3 localPos = pos - new Vector3(chunkPosition.x, 0, chunkPosition.y);

        // Calculate the normalized coordinates within the chunk
        float normX = Mathf.Clamp01(localPos.x / Utility.CHUNK_X);
        float normY = Mathf.Clamp01(localPos.y / Utility.CHUNK_Y);
        float normZ = Mathf.Clamp01(localPos.z / Utility.CHUNK_Z);

        // Convert normalized coordinates to chunk indices
        int x0 = Mathf.FloorToInt(normX * (Utility.CHUNK_X / xStep));
        int y0 = Mathf.FloorToInt(normY * (Utility.CHUNK_Y / yStep));
        int z0 = Mathf.FloorToInt(normZ * (Utility.CHUNK_Z / zStep));

        // Calculate interpolation weights
        //float wX = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1.0f - Mathf.Abs(2.0f * normX - 1.0f)));
        //float wY = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1.0f - Mathf.Abs(2.0f * normY - 1.0f)));
        //float wZ = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1.0f - Mathf.Abs(2.0f * normZ - 1.0f)));

        float wX = Mathf.SmoothStep(0f, 1f, 0.01f);
        float wY = Mathf.SmoothStep(0f, 1f, 0.01f);
        float wZ = Mathf.SmoothStep(0f, 1f, 0.01f);

        // Perform trilinear interpolation
        float sample000 = GetNoiseValue(x0, y0, z0);
        float sample001 = GetNoiseValue(x0, y0, z0 + 1);
        float sample010 = GetNoiseValue(x0, y0 + 1, z0);
        float sample011 = GetNoiseValue(x0, y0 + 1, z0 + 1);
        float sample100 = GetNoiseValue(x0 + 1, y0, z0);
        float sample101 = GetNoiseValue(x0 + 1, y0, z0 + 1);
        float sample110 = GetNoiseValue(x0 + 1, y0 + 1, z0);
        float sample111 = GetNoiseValue(x0 + 1, y0 + 1, z0 + 1);

        float interpolatedValue = Mathf.Lerp(
            Mathf.Lerp(Mathf.Lerp(sample000, sample100, wX), Mathf.Lerp(sample010, sample110, wX), wY),
            Mathf.Lerp(Mathf.Lerp(sample001, sample101, wX), Mathf.Lerp(sample011, sample111, wX), wY),
            wZ
        );

        return interpolatedValue;
    }

    private float GetNoiseValue(int x, int y, int z)
    {
        // Ensure the indices are within bounds
        x = Mathf.Clamp(x, 0, Utility.CHUNK_X / xStep - 1);
        y = Mathf.Clamp(y, 0, Utility.CHUNK_Y / yStep - 1);
        z = Mathf.Clamp(z, 0, Utility.CHUNK_Z / zStep - 1);

        int index = z * (Utility.CHUNK_X / xStep) * (Utility.CHUNK_Y / yStep) + y * (Utility.CHUNK_X / xStep) + x;
        return noiseMap[index];
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
