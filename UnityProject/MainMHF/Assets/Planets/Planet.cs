using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 256;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public Texture2D texture;
    //[Range(1, 10)]
    public float planetRadius = 1.0f;
    //[Range(0, 1)]
    public float heightScale = 0.1f;

    private void OnValidate()
    {
        Initialize();
        MeshData [] mds = GenerateMesh();
        if (GetComponent<PlanetNavMesh>() != null)
        {
            GetComponent<PlanetNavMesh>().OnChangePlanet(mds);
        }
    }

    void Initialize()
    {
        Vector3[][] directions = {
                                    new Vector3[]{ Vector3.up, Vector3.forward, Vector3.right },
                                    new Vector3[]{ Vector3.down, Vector3.right, Vector3.forward }
                                    //new Vector3[]{ Vector3.left },
                                    //new Vector3[]{ Vector3.right },
                                    //new Vector3[]{ Vector3.forward },
                                    //new Vector3[]{ Vector3.back }
                                  };

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[ directions.Length ];
        }
        terrainFaces = new TerrainFace[ directions.Length ];

        for (int i = 0; i < directions.Length; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(resolution, directions[i][0], directions[i][1], directions[i][2]);
        }
    }

    MeshData[] GenerateMesh()
    {
        MeshData [] mds = new MeshData[terrainFaces.Length];

        Vector3[] planet_vertices = new Vector3[resolution * resolution * terrainFaces.Length];
        Vector3[] planet_normals = new Vector3[planet_vertices.Length];
        Vector2[] planet_uvs = new Vector2[planet_vertices.Length];
        int[] planet_triangles = new int[(resolution - 1) * (resolution - 1) * 6 * terrainFaces.Length];

        print("Vertices: " + planet_vertices.Length + "  Triangles: " + planet_triangles.Length);

        int face_index = 0;
        foreach (TerrainFace face in terrainFaces)
        {
            mds[face_index] = face.CalculateTriangles(planetRadius, heightScale, texture);

            int startVertexIdx = face_index * resolution * resolution;
            int startTriangleIdx = face_index * (resolution - 1) * (resolution - 1) * 6;

            for (int vidx = 0; vidx < mds[face_index].vertices.Length; ++vidx)
            {
                planet_vertices[vidx + startVertexIdx] = mds[face_index].vertices[vidx];
                planet_normals[vidx + startVertexIdx] = mds[face_index].normals[vidx];
                planet_uvs[vidx + startVertexIdx] = mds[face_index].uvs[vidx];
            }

            for (int tidx = 0; tidx < mds[face_index].triangles.Length; ++tidx)
            {
                planet_triangles[tidx + startTriangleIdx] = mds[face_index].triangles[tidx] + startVertexIdx;
            }

            var mesh = meshFilters[face_index].sharedMesh;
            mesh.Clear();
            mesh.vertices = mds[face_index].vertices;
            mesh.normals = mds[face_index].normals;
            mesh.SetUVs(0, mds[face_index].uvs);
            mesh.triangles = mds[face_index].triangles;

            ++face_index;
        }

        /*var mesh = meshFilters[0].sharedMesh;
        mesh.Clear();
        mesh.vertices = planet_vertices;
        mesh.normals = planet_normals;
        mesh.SetUVs(0, planet_uvs);
        mesh.triangles = planet_triangles;*/
        //mesh.RecalculateNormals();

        return mds;
    }

}