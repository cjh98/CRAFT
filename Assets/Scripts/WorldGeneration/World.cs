using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    public Transform player;

    public static World instance;

    public Material material;

    public Vector3Int chunkDimensions;

    public GameObject chunkDataPrefab;
    public GameObject shaderDataPrefab;
    public GameObject chunkMeshPrefab;

    public Dictionary<Vector2Int, GameObject> chunkDataList = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, GameObject> chunkMeshList = new Dictionary<Vector2Int, GameObject>();

    private Queue<Vector2Int> chunksMeshesToGenerate = new Queue<Vector2Int>();
    private Queue<Vector2Int> chunksDataToGenerate = new Queue<Vector2Int>();

    public int range;

    private bool isCreatingChunkMeshes;
    private bool isCreatingChunkData;

    private Camera main;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        main = Camera.main;
        //Test();
    }

    #region TEST
    private void Test()
    {
        Vector2Int pos = new(0, 0);

        GameObject chunkData = Instantiate(shaderDataPrefab, new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), Quaternion.identity, transform);
        chunkData.GetComponent<ChunkShaderData>().position = new Vector2Int(pos.x * chunkDimensions.x, pos.y * chunkDimensions.z);

        chunkDataList[pos] = chunkData;
        chunksDataToGenerate.Enqueue(pos);

        GameObject chunkMesh = Instantiate(chunkMeshPrefab, new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), Quaternion.identity, transform);

        ChunkMesh mesh = chunkMesh.GetComponent<ChunkMesh>();

        //BurstChunkData dataObject = chunkData.GetComponent<BurstChunkData>();
        ChunkShaderData dataObject = chunkData.GetComponent<ChunkShaderData>();

        mesh.SetChunkData(dataObject);
        chunkMeshList[pos] = chunkMesh;
        chunksMeshesToGenerate.Enqueue(pos);

        dataObject.Init();
        mesh.Init(true);

    }
    #endregion

    private void Update()
    {
        UpdateWorld(range);

        if (chunksDataToGenerate.Count > 0 && !isCreatingChunkData)
        {
            StartCoroutine(nameof(CreateChunkData));
        }

        if (chunksMeshesToGenerate.Count > 0 && !isCreatingChunkMeshes)
        {
            StartCoroutine(nameof(CreateChunkMeshes));
        }

        StartCoroutine(nameof(DisableOrEnableChunks));
    }

    #region ASYNC
    private IEnumerator CreateChunkMeshes()
    {
        isCreatingChunkMeshes = true;

        while (chunksMeshesToGenerate.Count > 0)
        {
            Vector2Int index = chunksMeshesToGenerate.Dequeue();
            ChunkMesh mesh = chunkMeshList[index].GetComponent<ChunkMesh>();

            mesh.Init(true);

            yield return null;
        }

        isCreatingChunkMeshes = false;
    }

    private IEnumerator CreateChunkData()
    {
        isCreatingChunkData = true;

        while (chunksDataToGenerate.Count > 0)
        {
            Vector2Int index = chunksDataToGenerate.Dequeue();
            ChunkShaderData data = chunkDataList[index].GetComponent<ChunkShaderData>();

            data.Init();

            data.finished = true;

            yield return null;
        }

        isCreatingChunkData = false;
    }

    private IEnumerator DisableOrEnableChunks()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(main);

        foreach (Vector2Int pos in chunkMeshList.Keys)
        {
            GameObject chunk = chunkMeshList[pos];
            if (!GeometryUtility.TestPlanesAABB(planes, chunk.GetComponent<Renderer>().bounds))
            {
                chunk.SetActive(false);
                chunkDataList[pos].SetActive(false);
            }
            else
            {
                chunk.SetActive(true);
                chunkDataList[pos].SetActive(true);
            }
        }

        yield return null;
    }
    #endregion

    #region HELP
    private Vector2Int GetPlayerChunk()
    {
        return new Vector2Int(Mathf.FloorToInt(player.position.x / chunkDimensions.x), Mathf.FloorToInt(player.position.z / chunkDimensions.z));
    }

    public Vector2Int GetChunkAt(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkDimensions.x), Mathf.FloorToInt(pos.z / chunkDimensions.z));
    }

    public int WorldVector3ToChunkIndex(Vector3 pos)
    {
        Vector2Int chunkPos = GetChunkAt(pos);
        BurstChunkData data = chunkDataList[chunkPos].GetComponent<BurstChunkData>();

        Vector3Int posI = new Vector3Int(Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y),
            Mathf.FloorToInt(pos.z));

        posI = new Vector3Int(posI.x - chunkPos.x * chunkDimensions.x, posI.y, posI.z - chunkPos.y * chunkDimensions.z);

        return data.GetBlockIndex(posI.x, posI.y, posI.z);
    }

    public bool IsBlockAt(Vector3 pos)
    {
        Vector2Int chunkPos = GetChunkAt(pos);
        if (chunkMeshList.ContainsKey(chunkPos) && chunkDataList.ContainsKey(chunkPos))
        {
            BurstChunkData data = chunkDataList[chunkPos].GetComponent<BurstChunkData>();
            int index = WorldVector3ToChunkIndex(pos);

            if (index < data.blockMap.Length && data.finished)
            {
                return data.blockMap[index] != Utility.Blocks.Air;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public Utility.Blocks GetBlockAtVec3(Vector3 pos)
    {
        Vector2Int chunkPos = GetChunkAt(pos);
        BurstChunkData data = chunkDataList[chunkPos].GetComponent<BurstChunkData>();

        int index = WorldVector3ToChunkIndex(pos);

        return data.blockMap[index];
    }
    public void EditChunkBlockmap(Vector3 pos, Utility.Blocks newBlock)
    {
        Vector2Int chunk = GetChunkAt(pos);

        BurstChunkData data = chunkDataList[chunk].GetComponent<BurstChunkData>();
        ChunkMesh mesh = chunkMeshList[chunk].GetComponent<ChunkMesh>();

        int index = WorldVector3ToChunkIndex(pos);

        data.blockMap[index] = newBlock;
        mesh.Init(false);
    }
    #endregion

    private void UpdateWorld(int range)
    {
        Vector2Int playerChunk = GetPlayerChunk();

        for (int z = playerChunk.y - range; z < playerChunk.y + range; z++)
        {
            for (int x = playerChunk.x - range; x < playerChunk.x + range; x++)
            {
                Vector2Int pos = new Vector2Int(x, z);

                if (!chunkDataList.ContainsKey(pos))
                {
                    CreateChunkDatas(pos);
                }

                if (!chunkMeshList.ContainsKey(pos))
                {
                    CreateChunkMesh(pos);
                }
            }
        }
    }

    void CreateChunkDatas(Vector2Int pos)
    {
        GameObject chunkData = Instantiate(shaderDataPrefab, new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), Quaternion.identity, transform);
        chunkData.GetComponent<ChunkShaderData>().position = new Vector2Int(pos.x * chunkDimensions.x, pos.y * chunkDimensions.z);

        chunkDataList[pos] = chunkData;
        chunksDataToGenerate.Enqueue(pos);
    }

    void CreateChunkMesh(Vector2Int pos)
    {
        GameObject chunkMesh = Instantiate(chunkMeshPrefab, new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), Quaternion.identity, transform);

        if (chunkDataList.TryGetValue(pos, out GameObject chunkData))
        {
            ChunkMesh mesh = chunkMesh.GetComponent<ChunkMesh>();

            ChunkShaderData dataObject = chunkData.GetComponent<ChunkShaderData>();

            mesh.SetChunkData(dataObject);
            chunkMeshList[pos] = chunkMesh;
            chunksMeshesToGenerate.Enqueue(pos);
        }
    }
}
