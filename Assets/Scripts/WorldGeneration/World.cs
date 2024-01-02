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

    private int range = 16;

    private bool isCreatingChunkMeshes;
    private bool isCreatingChunkData;

    private void Awake()
    {
        instance = this;
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
    }

    private IEnumerator CreateChunkMeshes()
    {
        isCreatingChunkMeshes = true;

        while (chunksMeshesToGenerate.Count > 0)
        {
            Vector2Int index = chunksMeshesToGenerate.Dequeue();
            ChunkMesh mesh = chunkMeshList[index].GetComponent<ChunkMesh>();

            mesh.Init();

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

            yield return null;
        }

        isCreatingChunkData = false;
    }

    private Vector2Int GetPlayerChunk()
    {
        return new Vector2Int(Mathf.FloorToInt(player.position.x / chunkDimensions.x), Mathf.FloorToInt(player.position.z / chunkDimensions.z));
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
}
