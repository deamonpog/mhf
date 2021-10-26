using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public Vector3 [] vertices;
    public Vector2 [] uvs;
    public Vector3 [] normals;
    public int [] triangles;

    public MeshData(Vector3[] v, Vector2[] uv, Vector3[] n, int[] t)
    {
        vertices = v;
        uvs = uv;
        normals = n;
        triangles = t;
    }
};

public class TerrainFace
{
    int resolution;
    Vector3 localUp;
    Vector3 localRight;
    Vector3 localForward;

    public TerrainFace(int resolution, Vector3 localUp, Vector3 localRight, Vector3 localForward)
    {
        this.resolution = resolution;
        this.localUp = localUp;

        this.localRight = localRight;
        this.localForward = localForward;
    }

    public MeshData CalculateTriangles(float planetRadius, float heightScale ,Texture2D texture)
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * localRight + (percent.y - .5f) * 2 * localForward;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                Sc_SphericalCoord pointSC = Sc_SphericalCoord.FromCartesian(pointOnUnitSphere);

                float ty = ( 1.0f - (pointSC.polar / Mathf.PI));
                float tx = (pointSC.azimuthal / (2.0f * Mathf.PI));
                float c = texture.GetPixel((int)(tx * texture.width), (int)(ty * texture.height)).grayscale;

                pointOnUnitSphere *= (planetRadius + ((c >= 0.05f)?0.02f:0.0f) + c * heightScale);

                vertices[i] = pointOnUnitSphere;
                normals[i] = pointOnUnitCube.normalized;
                uvs[i] = new Vector2(tx, ty);

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        return new MeshData(vertices, uvs, normals, triangles); 
    }

    /*public void ConstructMesh(MeshData md)
    {
        mesh.Clear();
        mesh.vertices = md.vertices;
        mesh.SetUVs(0, md.uvs);
        mesh.triangles = md.triangles;
        mesh.RecalculateNormals();
    }*/
}