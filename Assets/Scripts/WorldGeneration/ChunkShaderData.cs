using UnityEngine;

public class ChunkShaderData : MonoBehaviour
{
    public Vector2Int position;

    public ComputeShader Shader2D;
    public ComputeShader Shader3D;

    [System.NonSerialized]
    public RenderTexture renderTexture2D;
    [System.NonSerialized]
    public RenderTexture renderTexture3D;

    public bool finished = false;

    public Utility.Blocks[] BlockMap { get; private set; }

    private float[] noiseMap;

    private int size = Utility.CHUNK_X * Utility.CHUNK_Y * Utility.CHUNK_Z;

    public void Init()
    {
        noiseMap = new float[size];
        BlockMap = new Utility.Blocks[size];

        DispatchComputeShaders();

        CreateWorldShape();
    }

    private void DispatchComputeShaders()
    {
        int kernelHandle2D = Shader2D.FindKernel("CSMain2D");
        int kernelHandle3D = Shader3D.FindKernel("CSMain3D");

        // 2D
        //Shader2D.SetFloat("scale", 50.0f);
        //Shader2D.SetFloat("heightScale", 1.0f);
        //Shader2D.SetFloat("octaves", 1);
        //Shader2D.SetFloat("lacunarity", 2.0f);
        //Shader2D.SetFloat("persistence", 0.5f);
        //Shader2D.SetFloat("offsetX", position.x);
        //Shader2D.SetFloat("offsetY", position.y);
        //Shader2D.SetFloat("seed", 69.0f);

        //Shader2D.SetTexture(kernelHandle2D, "heightMap2D", renderTexture2D);
        //Shader2D.Dispatch(kernelHandle2D, Utility.CHUNK_X / 8, Utility.CHUNK_Z / 8, 1);

        // 3D
        Shader3D.SetInt("width", Utility.CHUNK_X);
        Shader3D.SetInt("height", Utility.CHUNK_Y);
        Shader3D.SetFloat("heightScale", Utility.CHUNK_Y / 2);
        Shader3D.SetFloat("offsetX", position.x);
        Shader3D.SetFloat("offsetZ", position.y);
        Shader3D.SetFloat("seed", 69.0f);

        ComputeBuffer computeNoiseMap = new ComputeBuffer(size, sizeof(float));

        Shader3D.SetBuffer(kernelHandle3D, "heightMap3D", computeNoiseMap);
        Shader3D.Dispatch(kernelHandle3D, Utility.CHUNK_X / 8, Utility.CHUNK_Y / 8, Utility.CHUNK_Z / 8);

        computeNoiseMap.GetData(noiseMap);
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

    private void CreateWorldShape()
    {
        if (noiseMap.Length > 0)
        {
            for (int z = 0; z < Utility.CHUNK_Z; z++)
            {
                for (int y = 0; y < Utility.CHUNK_Y; y++)
                {
                    for (int x = 0; x < Utility.CHUNK_X; x++)
                    {
                        int index = x + Utility.CHUNK_X * (y + Utility.CHUNK_Y * z);

                        Squash(index, y);

                        // initial pass: solid vs air
                        if (noiseMap[index] < 0)
                        {
                            BlockMap[index] = Utility.Blocks.Air;
                        }
                        else
                        {
                            BlockMap[index] = Utility.Blocks.Stone;
                        }
                    }
                }
            }

        }
        else
        {
            Debug.LogError("noiseMap not created");
        }
    }

    private void Squash(int i, int y)
    {
        int halfPoint = Mathf.FloorToInt(Utility.CHUNK_Y * WorldNoiseSettings.DEFAULT_HEIGHT_OFFSET / 2);
        int distFromHalfPoint = Mathf.Abs(y - halfPoint);

        if (y < halfPoint)
        {
            noiseMap[i] = Mathf.FloorToInt(noiseMap[i] + WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
        }
        else if (y > halfPoint)
        {
            noiseMap[i] = Mathf.FloorToInt(noiseMap[i] - WorldNoiseSettings.SQUASH_FACTOR * distFromHalfPoint);
        }
    }
}
