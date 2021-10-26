using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetNavMesh : MonoBehaviour
{
    public Planet mPlanet;
    SortedSet<Vector3> vertices;
    List<List<int>> polygons;
    List<Mesh> meshes;

    public void OnChangePlanet(MeshData [] meshData)
    {
        /*meshes.Clear();
        polygons.Clear();

        List<Vector3[]> triangles = new List<Vector3[]>();

        for (int md_idx = 0; md_idx < meshData.Length; md_idx++)
        {
            for (int md_triangle_idx = 0; md_triangle_idx < meshData[md_idx].triangles.Length / 3; md_triangle_idx += 3)
            {
                Vector3 vertice0 = meshData[md_idx].vertices[ meshData[md_idx].triangles[md_triangle_idx + 0] ];
                Vector3 vertice1 = meshData[md_idx].vertices[meshData[md_idx].triangles[md_triangle_idx + 1]];
                Vector3 vertice2 = meshData[md_idx].vertices[meshData[md_idx].triangles[md_triangle_idx + 2]];
                Vector3[] triangle = new Vector3[] { vertice0, vertice1, vertice2 }; 
                triangles.Add(triangle);
            }
        }

        ;*/

    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            Gizmos.color = (i % 2 == 0)? Color.blue: Color.red;
            Gizmos.DrawMesh(meshes[i]);
        }
    }
}
