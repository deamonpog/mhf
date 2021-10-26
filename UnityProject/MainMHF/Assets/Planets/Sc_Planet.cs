using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

public class Sc_Planet : MonoBehaviour
{
    [Range(2, 512)]
    public int resolution = 2;

    [Range(2, 256)]
    public int planetRadius = 100;

    [Range(0, 1)]
    public double maxHeightRatioToRadius = 0.1;

    [Range(0, 1)]
    public double heightScaleToMax = 1f;

    public Texture2D texture;

    MeshFilter meshFilter;

    struct BasicMeshData
    {
        public Vector3[] vertices;
        public int [] triangles;

        public BasicMeshData(Vector3[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }

    [Button]
    void Initialize()
    {
        if (meshFilter == null)
        {
            GameObject meshObj = new GameObject("mesh_Planet");
            meshObj.transform.parent = transform;

            meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }

        BasicMeshData meshdata = GetBaseCube();

        Vector3[] vertices = new Vector3[meshdata.vertices.Length];
        Vector3[] normals = new Vector3[meshdata.vertices.Length];
        Vector3[] uvs = new Vector3[meshdata.vertices.Length];
        for (int i = 0; i < meshdata.vertices.Length; ++i)
        {
            normals[i] = meshdata.vertices[i].normalized; // this is also the point on unit sphere

            Sc_SphericalCoord pointSC = Sc_SphericalCoord.FromCartesian(normals[i]);
            float ty = (1.0f - (pointSC.polar / Mathf.PI));
            float tx = (pointSC.azimuthal / (2.0f * Mathf.PI));
            uvs[i] = new Vector2(tx, ty);

            float c = texture.GetPixel((int)(tx * texture.width), (int)(ty * texture.height)).grayscale;
            vertices[i] = normals[i] * ( (float)planetRadius + (float)(c * heightScaleToMax * maxHeightRatioToRadius * (double)planetRadius) );
        }
        
        var mesh = meshFilter.sharedMesh;
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.SetUVs(0, uvs);
        mesh.triangles = meshdata.triangles;
    }


    BasicMeshData GetBaseCube()
    {
        int xmax = resolution + 1;
        int ymax = resolution + 1;

        float dx = 1.0f / resolution;
        float dy = 1.0f / resolution;

        Vector3[] vertsTop = new Vector3[xmax * ymax];
        Vector3[] vertsRight = new Vector3[xmax * ymax];
        Vector3[] vertsFront = new Vector3[xmax * ymax];
        Vector3[] vertsBottom = new Vector3[xmax * ymax];
        Vector3[] vertsLeft = new Vector3[xmax * ymax];
        Vector3[] vertsBack = new Vector3[xmax * ymax];

        for (int y = 0; y < ymax; y++)
        {
            for (int x = 0; x < xmax; x++)
            {
                float px = dx * x - 0.5f;
                float py = dy * y - 0.5f;
                int t = x + y * xmax;

                vertsTop[t] = new Vector3(py, 0.5f, px);
                vertsRight[t] = new Vector3(px, py, 0.5f);
                vertsFront[t] = new Vector3(0.5f, px, py);

                vertsBottom[t] = new Vector3(px, -0.5f, py);
                vertsLeft[t] = new Vector3(py, px, -0.5f);
                vertsBack[t] = new Vector3(-0.5f, py, px);
            }
        }

        List<int> trianglesList = new List<int>();
        for (int y = 0; y < ymax - 1; ++y)
        {
            for (int x = 0; x < xmax; ++x)
            {
                if (x % xmax != xmax - 1)
                {
                    int f = x + y * xmax;

                    trianglesList.Add(f);
                    trianglesList.Add(f + 1);
                    trianglesList.Add(f + 1 + xmax);

                    trianglesList.Add(f);
                    trianglesList.Add(f + 1 + xmax);
                    trianglesList.Add(f + xmax);
                }
            }
        }

        List<Vector3> verts = new List<Vector3>();
        Dictionary<Vector3, int> vdict = new Dictionary<Vector3, int>();
        List<int> triangles = new List<int>();
        int nextIndex = 0;

        void addFace(Vector3 [] in_verts, List<int> in_triangles)
        { 
            for(int i = 0; i < in_verts.Length; ++i)
            {
                if (!vdict.ContainsKey(in_verts[i]))
                {
                    vdict.Add(in_verts[i], nextIndex);
                    verts.Add(in_verts[i]);
                    ++nextIndex;
                }
            }

            for(int i = 0; i < in_triangles.Count; ++i)
            {
                triangles.Add(vdict[in_verts[in_triangles[i]]]);
            }
        }

        addFace(vertsTop, trianglesList);
        addFace(vertsRight, trianglesList);
        addFace(vertsFront, trianglesList);
        addFace(vertsBottom, trianglesList);
        addFace(vertsLeft, trianglesList);
        addFace(vertsBack, trianglesList);

        return new BasicMeshData(verts.ToArray(), triangles.ToArray());
    }
}
