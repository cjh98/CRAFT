using UnityEngine;
using System;
using System.Collections.Generic;

public class ChunkMesh : MonoBehaviour
{
    private Mesh mesh;

    private MeshFilter meshFilter;

    private readonly List<Vector3> vertices = new();
    private readonly List<int> tris = new();
    private readonly List<Vector4> uvs = new();
    private readonly List<Vector3> normals = new();

    private BurstChunkData chunkData;

    private int vertexCount = 0;

    public Material material;

    public Vector2Int newPos;

    public struct Mask
    {
        public Utility.Blocks block;
        public int normal;

        public Mask(Utility.Blocks _block, int _normal)
        {
            block = _block;
            normal = _normal;
        }
    }

    public void SetChunkData(BurstChunkData data)
    {
        chunkData = data;
    }

    private void ClearArrays()
    {
        vertices.Clear();
        tris.Clear();
        normals.Clear();
        uvs.Clear();

        vertexCount = 0;
    }

    public void Init(bool firstGen)
    {
        meshFilter = GetComponent<MeshFilter>();

        if (firstGen)
        {
            WorldPopulator.PopulateWorld(chunkData);
        }

        ClearArrays();
        GenerateMesh();
        ApplyMesh();
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();

        for (int axis = 0; axis < 3; axis++)
        {
            int axis1 = (axis + 1) % 3;
            int axis2 = (axis + 2) % 3;

            int mainAxisLimit = World.instance.chunkDimensions[axis];
            int axis1Limit = World.instance.chunkDimensions[axis1];
            int axis2Limit = World.instance.chunkDimensions[axis2];

            Vector3Int deltaAxis1 = Vector3Int.zero;
            Vector3Int deltaAxis2 = Vector3Int.zero;

            Vector3Int chunkItr = Vector3Int.zero;
            Vector3Int axisMask = Vector3Int.zero;

            axisMask[axis] = 1;

            Mask[] mask = new Mask[axis1Limit * axis2Limit];

            // check each slice
            for (chunkItr[axis] = -1; chunkItr[axis] < mainAxisLimit;)
            {
                int n = 0;

                for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2])
                {
                    for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1])
                    {
                        Utility.Blocks currentBlock = chunkData.GetBlock(chunkItr);
                        Utility.Blocks compareBlock = chunkData.GetBlock(chunkItr + axisMask);

                        bool currentBlockOpaque = currentBlock != Utility.Blocks.Air;
                        bool compareBlockOpaque = compareBlock != Utility.Blocks.Air;

                        if (currentBlockOpaque == compareBlockOpaque)
                        {
                            mask[n++] = new Mask(Utility.Blocks.Air, 0);
                        }
                        else if (currentBlockOpaque)
                        {
                            mask[n++] = new Mask(currentBlock, 1);
                        }
                        else
                        {
                            mask[n++] = new Mask(compareBlock, -1);
                        }
                    }
                }

                ++chunkItr[axis];
                n = 0;

                // Generate mesh from mask
                for (int j = 0; j < axis2Limit; j++)
                {
                    for (int i = 0; i < axis1Limit;)
                    {
                        if (mask[n].normal != 0)
                        {
                            Mask currentMask = mask[n];
                            chunkItr[axis1] = i;
                            chunkItr[axis2] = j;

                            int width;

                            for (width = 1; i + width < axis1Limit && CompareMask(mask[n + width], currentMask); width++) { }

                            int height;
                            bool done = false;

                            for (height = 1; j + height < axis2Limit; height++)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    if (CompareMask(mask[n + k + height * axis1Limit], currentMask)) continue;

                                    done = true;
                                    break;
                                }

                                if (done) break;
                            }

                            deltaAxis1[axis1] = width;
                            deltaAxis2[axis2] = height;

                            CreateQuad(currentMask, axisMask, chunkItr,
                                chunkItr + deltaAxis1,
                                chunkItr + deltaAxis2,
                                chunkItr + deltaAxis1 + deltaAxis2);

                            deltaAxis1 = Vector3Int.zero;
                            deltaAxis2 = Vector3Int.zero;

                            for (int l = 0; l < height; l++)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    mask[n + k + l * axis1Limit] = new Mask(Utility.Blocks.Air, 0);
                                }
                            }

                            i += width;
                            n += width;
                        }
                        else
                        {
                            i++;
                            n++;
                        }
                    }
                }
            }
        }
    }

    private void CreateQuad(Mask mask, Vector3 axisMask, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        Vector3 normal = axisMask * mask.normal;

        vertices.Add(v1); // bottom left
        vertices.Add(v2); // top left
        vertices.Add(v3); // top right
        vertices.Add(v4); // bottom right

        tris.Add(vertexCount + 1 + mask.normal);
        tris.Add(vertexCount + 1 - mask.normal);
        tris.Add(vertexCount + 3);
        tris.Add(vertexCount + 2 - mask.normal);
        tris.Add(vertexCount + 2 + mask.normal);
        tris.Add(vertexCount);

        int index = BlockToTexture(mask.block, axisMask);

        if (axisMask == Vector3.left || axisMask == Vector3.right)
        {
            uvs.Add(new Vector3(v1.z, v1.y, index));
            uvs.Add(new Vector3(v2.z, v2.y, index));
            uvs.Add(new Vector3(v3.z, v3.y, index));
            uvs.Add(new Vector3(v4.z, v4.y, index));
        }
        else if (axisMask == Vector3.up)
        {
            uvs.Add(new Vector3(v1.x, v1.z, index));
            uvs.Add(new Vector3(v2.x, v2.z, index));
            uvs.Add(new Vector3(v3.x, v3.z, index));
            uvs.Add(new Vector3(v4.x, v4.z, index));
        }
        else if (axisMask == Vector3.down)
        {
            uvs.Add(new Vector3(v1.x, v1.z, index));
            uvs.Add(new Vector3(v2.x, v2.z, index));
            uvs.Add(new Vector3(v3.x, v3.z, index));
            uvs.Add(new Vector3(v4.x, v4.z, index));
        }
        else
        {
            uvs.Add(new Vector3(v1.x, v1.y, index));
            uvs.Add(new Vector3(v2.x, v2.y, index));
            uvs.Add(new Vector3(v3.x, v3.y, index));
            uvs.Add(new Vector3(v4.x, v4.y, index));
        }

        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        vertexCount += 4;
    }

    private void ApplyMesh()
    {
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        World.instance.material.SetVectorArray("_UVs", uvs);

        mesh.SetTriangles(tris, 0);

        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
    }

    private bool CompareMask(Mask m1, Mask m2)
    {
        return m1.block == m2.block && m1.normal == m2.normal;
    }

    // move this somewhere or something
    private int BlockToTexture(Utility.Blocks block, Vector3 face)
    {
        return (block, face) switch
        {
            (Utility.Blocks.Stone, _) => 0,
            (Utility.Blocks.Dirt, _) => 1,
            (Utility.Blocks.Grass, Vector3 v) when v == Vector3.left || v == Vector3.right => 2,
            (Utility.Blocks.Grass, Vector3 v) when v == Vector3.up => 7,
            (Utility.Blocks.Grass, Vector3 v) when v == Vector3.down => 1,
            (Utility.Blocks.Grass, _) => 2,
            (Utility.Blocks.Sand, _) => 10,
            _ => 9
        };
    }
}