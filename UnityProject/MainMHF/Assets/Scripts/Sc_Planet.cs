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

    public Texture2D texture_heightMap;
    public Material material;

    private List<Mesh> navMesh;

    [Range(2, 30)]
    public int navMeshResolution = 10;

    public float navHeightDiff = 100.0f;

    public bool showNavMesh = false;

    struct BasicMeshData
    {
        public Vector3[] vertices;
        public int [] triangles;
        public Vector3[] normals;
        public Vector3[] uvs;

        public BasicMeshData(Vector3[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            normals = null;
            uvs = null;
        }
    }

    struct SortedTwoIntegers
    {
        public int A;
        public int B;

        public SortedTwoIntegers(int iA, int iB)
        {
            if (iA < iB)
            {
                this.A = iA;
                this.B = iB;
            }
            else
            {
                this.A = iB;
                this.B = iA;
            }
        }

        public override string ToString()
        {
            return string.Format("<{0},{1}>", A, B);
        }
    }

    class TwoIntegersComparer : IEqualityComparer<SortedTwoIntegers>
    {
        public bool Equals(SortedTwoIntegers x, SortedTwoIntegers y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(SortedTwoIntegers obj)
        {
            return obj.A + obj.B * 100;
        }
    }

    [Button]
    void RecalculateAndGenerate()
    {
        UnityEditor.EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", 0.0f);

        // Calculate the mesh triangles and vertices for a unit cube at origin
        BasicMeshData planet_GeomMesh = GetBaseCube(resolution);
        planet_GeomMesh = CalculateWithPlanetHeightMap(planet_GeomMesh);
        CreateMesh(planet_GeomMesh);

        // Calculate the navigational mesh for the planet
        BasicMeshData planet_NavMesh = GetBaseCube(navMeshResolution);
        planet_NavMesh = CalculateWithPlanetHeightMap(planet_NavMesh);
        GenerateNavMesh(planet_NavMesh);

        UnityEditor.EditorUtility.ClearProgressBar();
    }

    Mesh CreateMesh(BasicMeshData in_UnitCubeMeshData)
    {
        string planet_gameObjectName = "mesh_Planet";
        Transform planet_transform = transform.Find(planet_gameObjectName);
        GameObject planet_GameObject;
        MeshFilter planet_meshFilter;
        MeshRenderer planet_meshRenderer;

        // Get Mesh Child Object
        if (planet_transform == null)
        {
            planet_GameObject = new GameObject(planet_gameObjectName);
            planet_GameObject.transform.parent = this.transform;
        }
        else
        {
            planet_GameObject = planet_transform.gameObject;
        }

        // Get MeshRenderer of the Mesh Child Object
        if (planet_GameObject.GetComponent<MeshRenderer>() == null)
        {
            planet_meshRenderer = planet_GameObject.AddComponent<MeshRenderer>();
        }
        else
        {
            planet_meshRenderer = planet_GameObject.GetComponent<MeshRenderer>();
        }

        // Set material on the MeshRenderer
        planet_meshRenderer.sharedMaterial = new Material(material);

        // Get MeshFilter of the Mesh Child Object
        if (planet_GameObject.GetComponent<MeshFilter>() == null)
        {
            planet_meshFilter = planet_GameObject.AddComponent<MeshFilter>();
            planet_meshFilter.sharedMesh = new Mesh();
        }
        else
        {
            planet_meshFilter = planet_GameObject.GetComponent<MeshFilter>();
        }

        // Set the mesh values
        var mesh = planet_meshFilter.sharedMesh;
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = in_UnitCubeMeshData.vertices;
        mesh.normals = in_UnitCubeMeshData.normals;
        mesh.SetUVs(0, in_UnitCubeMeshData.uvs);
        mesh.triangles = in_UnitCubeMeshData.triangles;

        return mesh;
    }

    BasicMeshData CalculateWithPlanetHeightMap(BasicMeshData in_UnitCubeMeshData)
    {
        // Calculate the vertices, normals, uvs of the planet based on heightmap
        Vector3[] vertices = new Vector3[in_UnitCubeMeshData.vertices.Length];
        Vector3[] normals = new Vector3[in_UnitCubeMeshData.vertices.Length];
        Vector3[] uvs = new Vector3[in_UnitCubeMeshData.vertices.Length];
        for (int i = 0; i < in_UnitCubeMeshData.vertices.Length; ++i)
        {
            normals[i] = in_UnitCubeMeshData.vertices[i].normalized; // this is also the point on unit sphere

            Sc_SphericalCoord pointSC = Sc_SphericalCoord.FromCartesian(normals[i]);
            float ty = (1.0f - (pointSC.polar / Mathf.PI));
            float tx = (pointSC.azimuthal / (2.0f * Mathf.PI));
            uvs[i] = new Vector2(tx, ty);

            float c = texture_heightMap.GetPixel((int)(tx * texture_heightMap.width), (int)(ty * texture_heightMap.height)).grayscale;
            vertices[i] = normals[i] * ((float)planetRadius + (float)(c * heightScaleToMax * maxHeightRatioToRadius * (double)planetRadius));
        }

        BasicMeshData returnValue = new BasicMeshData(vertices, in_UnitCubeMeshData.triangles);
        returnValue.normals = normals;
        returnValue.uvs = uvs;

        return returnValue;
    }

    Vector3 calculateCenter(BasicMeshData mesh, int v0, int v1, int v2)
    {
        Vector3 vA0 = mesh.vertices[v0];
        Vector3 vA1 = mesh.vertices[v1];
        Vector3 vA2 = mesh.vertices[v2];
        Vector3 c = (vA0 + vA1 + vA2) / 3.0f;
        return c;
    }

    BasicMeshData GetBaseCube(int in_resolution)
    {
        int xmax = in_resolution + 1;
        int ymax = in_resolution + 1;

        float dx = 1.0f / in_resolution;
        float dy = 1.0f / in_resolution;

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

    void GenerateNavMesh(BasicMeshData in_BasicMeshData)
    {
        // Destroy existing navmesh and create a new one.
        if (navMesh == null)
        {
            navMesh = new List<Mesh>();
        }
        else
        {
            foreach (var m in navMesh)
            {
                m.Clear();
                DestroyImmediate(m);
            }
            navMesh.Clear();
        }

        // the full edge list
        List<SortedTwoIntegers> edgeList = new List<SortedTwoIntegers>();

        // triangles and their groups
        //Dictionary<int, int> triangle_to_group = new Dictionary<int, int>();
        Dictionary<int, List<int>> group_to_triangle = new Dictionary<int, List<int>>();

        // mapping of triangle to its group. This varible is useless after removing the group aliasing.
        Dictionary<int, int> temp_triangle_2_group = new Dictionary<int, int>();

        // indexes that represent the same group of triangles. This variable is useless after removing the aliasing.
        HashSet<SortedTwoIntegers> temp_sameGroup = new HashSet<SortedTwoIntegers>();

        // following is used for finding triangles with shared edge. This variable is useless after the following loop.
        Dictionary<SortedTwoIntegers, int> temp_edge_to_triangle = new Dictionary<SortedTwoIntegers, int>(new TwoIntegersComparer());

        // Calculate the basic edge list of connected triangles in the planet mesh on approximately similar height
        int cur_triangle = 0;
        int nextGroup = 0;
        for (int i = 0; i < in_BasicMeshData.triangles.Length; i += 3)
        {
            int v0 = in_BasicMeshData.triangles[i];
            int v1 = in_BasicMeshData.triangles[i + 1];
            int v2 = in_BasicMeshData.triangles[i + 2];

            Vector3 curTriCenter = calculateCenter(in_BasicMeshData, v0, v1, v2);
            float curHeight = curTriCenter.magnitude;

            SortedTwoIntegers[] triangle_edges = new SortedTwoIntegers[]{new SortedTwoIntegers(v0, v1), new SortedTwoIntegers(v0, v2), new SortedTwoIntegers(v1, v2)};

            //print(string.Format("T{0}", cur_triangle));
            //string msg = "Edges: ";
            //foreach(var x in triangle_edges) { msg += x; }
            //print(msg + " -> " + cur_triangle);

            foreach (SortedTwoIntegers sti_edge in triangle_edges)
            {
                if (temp_edge_to_triangle.ContainsKey(sti_edge))
                {
                    //print(string.Format("E{0} is in e2t",sti_edge));
                    int otherTriangle = temp_edge_to_triangle[sti_edge];
                    //print(string.Format("Other is T{0}", otherTriangle));

                    Vector3 otherTriCenter = calculateCenter(in_BasicMeshData, in_BasicMeshData.triangles[otherTriangle * 3], in_BasicMeshData.triangles[otherTriangle * 3 + 1], in_BasicMeshData.triangles[otherTriangle * 3 + 2]);

                    //if(otherTriangle == 11 | otherTriangle == 12 | cur_triangle == 11 | cur_triangle == 12) { print(string.Format("T{0} => T{1} Height diff: {2}", cur_triangle, otherTriangle, Mathf.Abs(otherTriCenter.magnitude - curHeight))); }
                    if(Mathf.Abs(otherTriCenter.magnitude - curHeight) < navHeightDiff)
                    {
                        //print("same height");
                        edgeList.Add(new SortedTwoIntegers(cur_triangle, otherTriangle));
                        if (temp_triangle_2_group.ContainsKey(cur_triangle))
                        {
                            //print("t2g contains CUR key");
                            if (temp_triangle_2_group.ContainsKey(otherTriangle))
                            {
                                //print("t2g contains CUR and OTHER keys and they are " + ((temp_triangle_2_group[cur_triangle] == temp_triangle_2_group[otherTriangle])? "Equal" : "Not Equal"));
                                //print(string.Format("T{0} is in G{1}", cur_triangle, temp_triangle_2_group[cur_triangle]));
                                //print(string.Format("T{0} is in G{1}", otherTriangle, temp_triangle_2_group[otherTriangle]));
                                if (temp_triangle_2_group[cur_triangle] != temp_triangle_2_group[otherTriangle])
                                {
                                    var equalGrups = new SortedTwoIntegers(temp_triangle_2_group[cur_triangle], temp_triangle_2_group[otherTriangle]);
                                    //print(string.Format("Equal Groups : {0}", equalGrups));
                                    temp_sameGroup.Add(equalGrups);
                                }
                            }
                            else
                            {
                                //print("t2g contains CUR key only.");
                                //print(string.Format("T{0} -> G{1}", otherTriangle, temp_triangle_2_group[cur_triangle]));
                                temp_triangle_2_group.Add(otherTriangle, temp_triangle_2_group[cur_triangle]);
                            }
                        }
                        else if(temp_triangle_2_group.ContainsKey(otherTriangle))
                        {
                            //print("t2g contains OTHER key only.");
                            //print(string.Format("T{0} -> G{1}", cur_triangle, temp_triangle_2_group[otherTriangle]));
                            temp_triangle_2_group.Add(cur_triangle, temp_triangle_2_group[otherTriangle]);
                        }
                        else
                        {
                            //print("t2g none of the keys exist, adding to it.");
                            //print(string.Format("T{0} -> G{1}", cur_triangle, nextGroup));
                            //print(string.Format("T{0} -> G{1}", otherTriangle, nextGroup));
                            temp_triangle_2_group.Add(cur_triangle, nextGroup);
                            temp_triangle_2_group.Add(otherTriangle, nextGroup);
                            ++nextGroup;
                        }
                    }

                    temp_edge_to_triangle.Remove(sti_edge);
                }
                else
                {
                    //print(string.Format("E{0} is not in e2t, adding to it.", sti_edge));
                    temp_edge_to_triangle.Add(sti_edge, cur_triangle);
                }
            }

            ++cur_triangle;
        }

        //print("=============================== T -> G =======================================");
        //foreach (var x in temp_triangle_2_group) { print(string.Format("T{0} -> G{1}", x.Key, x.Value)); }
        print(string.Format("E_to_T : {0}", temp_edge_to_triangle.Count));
        print(string.Format("EdgeList : {0}", edgeList.Count));
        print(string.Format("SameGroup : {0}", temp_sameGroup.Count));

        //print("=============================== Same Groups =======================================");
        //print(temp_sameGroup.Count);
        //foreach (var x in temp_sameGroup) { print(x); }
        temp_edge_to_triangle = null;

        print("=============================== Removing Aliases =======================================");

        Dictionary<int,HashSet<int>> groupSetDict = new Dictionary<int,HashSet<int>>();

        // replace indexes that represent the same group with a single index
        int setId = 0;
        foreach (SortedTwoIntegers aliasedGroups in temp_sameGroup)
        {
            int id_a = -1;
            int id_b = -1;
            foreach(var g in groupSetDict)
            {
                if (g.Value.Contains(aliasedGroups.A))
                {
                    id_a = g.Key;
                }
                if (g.Value.Contains(aliasedGroups.B))
                {
                    id_b = g.Key;
                }
            }

            if(id_a == -1)
            {
                // A is not in a set.
                if (id_b == -1)
                {
                    // B is not in a set.
                    //print(string.Format("Adding new set : {0}", aliasedGroups));
                    groupSetDict.Add(setId, new HashSet<int>(new int[] { aliasedGroups.A, aliasedGroups.B }));
                    ++setId;
                }
                else
                {
                    // B is in a set.
                    //print(string.Format("Adding {0} to base {1}", aliasedGroups.A, aliasedGroups.B));
                    groupSetDict[id_b].Add(aliasedGroups.A);
                }
            }
            else
            {
                // A is in a set.
                if (id_b == -1)
                {
                    // B is not in a set.
                    //print(string.Format("Adding {0} to base {1}", aliasedGroups.B, aliasedGroups.A));
                    groupSetDict[id_a].Add(aliasedGroups.B);
                }
                else
                {
                    // B is in a set.
                    if(id_a == id_b)
                    {
                        //print("Both are in the same set. No need of change.");
                    }
                    else
                    {
                        //print("Both are in different sets. Taking union.");
                        groupSetDict[id_a].UnionWith(groupSetDict[id_b]);
                        groupSetDict.Remove(id_b);
                    }
                }
            }
        }
        temp_sameGroup = null;

        print(string.Format("Group Sets: {0}", groupSetDict.Count));

        // map of new indexes that represent group numbers without aliases
        Dictionary<int, int> temp_groups_to_newGroups = new Dictionary<int, int>();

        foreach(var groupSet in groupSetDict)
        {
            foreach(var gr in groupSet.Value)
            {
                temp_groups_to_newGroups.Add(gr, nextGroup);
            }

            ++nextGroup;
        }
        groupSetDict = null;

        print("================ Stranded Triangles ================");
        HashSet<int> groupedTriangles = new HashSet<int>(System.Linq.Enumerable.Range(0, in_BasicMeshData.triangles.Length / 3));
        groupedTriangles.ExceptWith(temp_triangle_2_group.Keys);
        foreach (int t in groupedTriangles)
        {
            temp_triangle_2_group.Add(t, nextGroup);
            ++nextGroup;
        }

        print("========== Group Calculation Done =============");
        print(temp_groups_to_newGroups.Count);
        //foreach (var x in temp_groups_to_newGroups) { print(x); }

        print("========== Reassigning Groups =============");
        // replace the old group indexes by the recalculated new group indexes
        foreach (var tri_to_grp in temp_triangle_2_group)
        {
            //print(string.Format("Processing T{0} => G{1}",tri_to_grp.Key, tri_to_grp.Value));

            int recalculatedGroupId = (temp_groups_to_newGroups.ContainsKey(tri_to_grp.Value)) ? temp_groups_to_newGroups[tri_to_grp.Value] : tri_to_grp.Value;
            //triangle_to_group.Add(tri_to_grp.Key, newGroup);
            if (!group_to_triangle.ContainsKey(recalculatedGroupId))
            {
                group_to_triangle.Add(recalculatedGroupId, new List<int>());
            }
            group_to_triangle[recalculatedGroupId].Add(tri_to_grp.Key);
        }
        print(string.Format("grps to newGrps : {0}", temp_groups_to_newGroups.Count));
        temp_groups_to_newGroups = null;

        print(string.Format("Groups : {0}", group_to_triangle.Count));

        // Create navmesh child if it doesn't exist
        string naveMeshName = "navMesh";
        Transform navMeshTransform = transform.Find(naveMeshName);
        GameObject navMeshGO;
        if(navMeshTransform == null)
        {
            navMeshGO = new GameObject(naveMeshName);
            navMeshGO.transform.parent = gameObject.transform;
        }
        else
        {
            navMeshGO = navMeshTransform.gameObject;
        }

        // Destroy all children in navmesh
        while (navMeshGO.transform.childCount > 0)
        {
            DestroyImmediate(navMeshGO.transform.GetChild(0).gameObject);
        }

        Color[] colorArray = new Color[] { Color.green, Color.cyan, Color.blue, Color.yellow, Color.red, Color.magenta, Color.white };

        int nameIdx = 0;
        foreach (var grpTris in group_to_triangle)
        {
            int[] tris = new int[3 * grpTris.Value.Count];
            for(int i = 0; i < grpTris.Value.Count; ++i)
            {
                int triIdx = grpTris.Value[i];

                var v0 = in_BasicMeshData.triangles[triIdx * 3];
                var v1 = in_BasicMeshData.triangles[triIdx * 3 + 1];
                var v2 = in_BasicMeshData.triangles[triIdx * 3 + 2];

                tris[i * 3] = v0;
                tris[(i * 3) + 1] = v1;
                tris[(i * 3) + 2] = v2;
            }

            Mesh m = new Mesh();
            m.Clear();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = in_BasicMeshData.vertices;
            m.normals = in_BasicMeshData.normals;
            m.SetUVs(0, in_BasicMeshData.uvs);
            m.triangles = tris;

            navMesh.Add(m);

            var go = new GameObject(string.Format("M_{0}_G{1}", nameIdx, grpTris.Key));
            go.transform.parent = navMeshGO.transform;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            // load color material for navmesh displaying
            Material yourMaterial = new Material(Shader.Find("Standard"));
            float h = (float)(nameIdx + 1) / (float)group_to_triangle.Count;
            yourMaterial.color = (nameIdx >= colorArray.Length)? Color.HSVToRGB(h, 1f, 1f) : colorArray[nameIdx];
            mr.material = yourMaterial;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = m;
            go.transform.localScale = Vector3.one * 1.1f;

            ++nameIdx;
        }

        print("NavMeshes : " + navMesh.Count);
    }

    void OnValidate()
    {
        Transform navMeshTransform = transform.Find("navMesh");
        if (navMeshTransform != null)
        {
            GameObject navMeshGO = navMeshTransform.gameObject;
            navMeshGO.SetActive(showNavMesh);
        }
    }
}
