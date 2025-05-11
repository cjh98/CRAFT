using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class World : MonoBehaviour
{
    public Transform player;

    public static World instance;

    public Material material;

    public Vector3Int chunkDimensions;

    public GameObject chunkDataPrefab;
    public GameObject shaderDataPrefab;
    public GameObject chunkMeshPrefab;

    public Dictionary<Vector2Int, BurstChunkData> chunkDataList = new();
    public Dictionary<Vector2Int, ChunkMesh> chunkMeshList = new();

    private readonly Queue<Vector2Int> chunksMeshesToGenerate = new();
    private readonly Queue<Vector2Int> chunksDataToGenerate = new();

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
    }

    private void Update()
    {
        UpdateWorld();

        // Always make chunk data
        if (chunksDataToGenerate.Count > 0 && !isCreatingChunkData)
        {
            StartCoroutine(nameof(CreateChunkData));
        }

        // Make new meshes when not making new data
        if (chunksMeshesToGenerate.Count > 0 && !isCreatingChunkMeshes && !isCreatingChunkData)
        {
            StartCoroutine(nameof(CreateChunkMeshes));
        }

        // frustrum culling
        StartCoroutine(nameof(DisableOrEnableChunks));
    }

    #region ASYNC
    private IEnumerator CreateChunkMeshes()
    {
        isCreatingChunkMeshes = true;

        while (chunksMeshesToGenerate.Count > 0)
        {
            Vector2Int index = chunksMeshesToGenerate.Dequeue();

            chunkMeshList[index].Init(true);

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

            chunkDataList[index].Init();

            chunkDataList[index].finished = true;

            yield return null;
        }

        isCreatingChunkData = false;
    }

    private IEnumerator DisableOrEnableChunks()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(main);

        foreach (Vector2Int pos in chunkMeshList.Keys)
        {
            GameObject chunk = chunkMeshList[pos].gameObject;
            if (!GeometryUtility.TestPlanesAABB(planes, chunk.GetComponent<Renderer>().bounds))
            {
                chunk.SetActive(false);
                chunkDataList[pos].gameObject.SetActive(false);
            }
            else
            {
                chunk.SetActive(true);
                chunkDataList[pos].gameObject.SetActive(true);
            }
        }

        yield return null;
    }
    #endregion

    

    private void UpdateWorld()
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
        GameObject chunkObject = Instantiate(chunkDataPrefab, 
            new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), 
            Quaternion.identity, 
            transform);

        var chunkData = chunkObject.GetComponent<BurstChunkData>();
        chunkData.position = new Vector2Int(pos.x * chunkDimensions.x, pos.y * chunkDimensions.z);

        chunkDataList[pos] = chunkData;
        chunksDataToGenerate.Enqueue(pos);
    }

    void CreateChunkMesh(Vector2Int pos)
    {
        GameObject chunkObject = Instantiate(
            chunkMeshPrefab, 
            new Vector3(pos.x * chunkDimensions.x, 0, pos.y * chunkDimensions.z), 
            Quaternion.identity, transform);

        if (chunkDataList.TryGetValue(pos, out BurstChunkData chunkData))
        {
            ChunkMesh mesh = chunkObject.GetComponent<ChunkMesh>();

            mesh.SetChunkData(chunkData);
            chunkMeshList[pos] = mesh;
            chunksMeshesToGenerate.Enqueue(pos);
        }
    }

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
        BurstChunkData data = chunkDataList[chunkPos];

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
            BurstChunkData data = chunkDataList[chunkPos];
            int index = WorldVector3ToChunkIndex(pos);

            if (index < data.BlockMap.Length && data.finished)
            {
                return data.BlockMap[index] != Utility.Blocks.Air;
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
        BurstChunkData data = chunkDataList[chunkPos];

        int index = WorldVector3ToChunkIndex(pos);

        return data.BlockMap[index];
    }
    public void EditChunkBlockmap(Vector3 pos, Utility.Blocks newBlock)
    {
        Vector2Int chunk = GetChunkAt(pos);

        BurstChunkData data = chunkDataList[chunk];
        ChunkMesh mesh = chunkMeshList[chunk];

        int index = WorldVector3ToChunkIndex(pos);

        data.BlockMap[index] = newBlock;
        mesh.Init(false);
    }
    #endregion
}