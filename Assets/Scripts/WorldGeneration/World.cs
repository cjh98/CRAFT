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
    public GameObject chunkMeshPrefab;

    private Dictionary<Vector2Int, GameObject> chunkDataList = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> chunkMeshList = new Dictionary<Vector2Int, GameObject>();

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
    }

    private void Update()
    {
        UpdateWorld(range);

        if (chunksDataToGenerate.Count > 0 && !isCreatingChunkData)
        {
            StartCoroutine("CreateChunkData");
        }

        if (chunksMeshesToGenerate.Count > 0 && !isCreatingChunkMeshes)
        {
            StartCoroutine("CreateChunkMeshes");
        }

        StartCoroutine("DisableOrEnableChunks");
    }

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
            BurstChunkData data = chunkDataList[index].GetComponent<BurstChunkData>();

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
                chunk.gameObject.SetActive(false);
                chunkDataList[pos].gameObject.SetActive(false);
            }
            else
            {
                chunk.gameObject.SetActive(true);
                chunkDataList[pos].gameObject.SetActive(true);
            }
        }

        yield return null;
    }

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
        BurstChunkData data = null; 

        if (chunkMeshList.ContainsKey(chunkPos) && chunkDataList.ContainsKey(chunkPos))
        {
            data = chunkDataList[chunkPos].GetComponent<BurstChunkData>();

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
                    GameObject chunkData = Instantiate(chunkDataPrefab, new Vector3(x * chunkDimensions.x, 0, z * chunkDimensions.z), Quaternion.identity, transform);
                    chunkData.GetComponent<BurstChunkData>().position = new Vector2Int(x * chunkDimensions.x, z * chunkDimensions.z);
                    chunkDataList[pos] = chunkData;
                    chunksDataToGenerate.Enqueue(pos);
                }

                if (!chunkMeshList.ContainsKey(pos))
                {
                    GameObject chunkMesh = Instantiate(chunkMeshPrefab, new Vector3(x * chunkDimensions.x, 0, z * chunkDimensions.z), Quaternion.identity, transform);
                    GameObject chunkData;

                    if (chunkDataList.TryGetValue(pos, out chunkData))
                    {
                        ChunkMesh mesh = chunkMesh.GetComponent<ChunkMesh>();

                        mesh.SetChunkData(chunkData.GetComponent<BurstChunkData>());
                        chunkMeshList[pos] = chunkMesh;
                        chunksMeshesToGenerate.Enqueue(pos);
                    }
                }
            }
        }
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
}
