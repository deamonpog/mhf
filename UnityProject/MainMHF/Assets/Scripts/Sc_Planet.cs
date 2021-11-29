using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

public partial class Sc_Planet : MonoBehaviour
{
    [Range(2, 512)]
    public int resolution = 100;

    [Range(2, 256)]
    public int planetRadius = 50;

    [Range(0, 1)]
    public double maxHeightRatioToRadius = 0.1;

    [Range(0, 1)]
    public double heightScaleToMax = 1f;

    public Texture2D texture_heightMap;
    public Material material;

    [Range(2, 30)]
    public int navMeshResolution = 10;

    public float navHeightDiff = 1.0f;

    public Sc_NavMesh navMesh;

    public bool showNavMesh = false;

    struct BasicMeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
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

    [Button]
    void RecalculateAndGenerate()
    {
        //UnityEditor.EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", 0.0f);

        // Calculate the mesh triangles and vertices for a unit cube at origin
        BasicMeshData planet_GeomMesh = GetBaseCube(resolution);
        planet_GeomMesh = CalculateWithPlanetHeightMap(planet_GeomMesh);
        CreateMesh(planet_GeomMesh);

        // Calculate the navigational mesh for the planet
        BasicMeshData planet_NavMesh = GetBaseCube(navMeshResolution);
        planet_NavMesh = CalculateWithPlanetHeightMap(planet_NavMesh);
        navMesh = GenerateNavMesh(planet_NavMesh);

        //UnityEditor.EditorUtility.ClearProgressBar();
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
            planet_GameObject.layer = 8; // set planet layer
            planet_GameObject.tag = "Planet";
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

        // Add mesh collider component
        if (planet_GameObject.GetComponent<MeshCollider>() == null)
        {
            MeshCollider planet_meshCollider = planet_GameObject.AddComponent<MeshCollider>();
        }

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

        void addFace(Vector3[] in_verts, List<int> in_triangles)
        {
            for (int i = 0; i < in_verts.Length; ++i)
            {
                if (!vdict.ContainsKey(in_verts[i]))
                {
                    vdict.Add(in_verts[i], nextIndex);
                    verts.Add(in_verts[i]);
                    ++nextIndex;
                }
            }

            for (int i = 0; i < in_triangles.Count; ++i)
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

    GameObject refreshGameObject(string in_goname)
    {
        Transform graphTransform = transform.Find(in_goname);
        GameObject graphgo;
        if (graphTransform == null)
        {
            graphgo = new GameObject("Graph");
            graphgo.transform.parent = gameObject.transform;
        }
        else
        {
            graphgo = graphTransform.gameObject;
        }
        // Destroy all children in graph
        while (graphgo.transform.childCount > 0)
        {
            DestroyImmediate(graphgo.transform.GetChild(0).gameObject);
        }
        return graphgo;
    }

    Sc_NavMesh GenerateNavMesh(BasicMeshData in_BasicMeshData)
    {
        // List of Polygons with an index for each polygon as the key
        Dictionary<int, Sc_Polygon> temp_indexed_polygons_dict = new Dictionary<int, Sc_Polygon>();

        // Old invalid polygon indexes
        HashSet<int> temp_old_polygons = new HashSet<int>();

        // Used for finding polygons with shared edge.
        Dictionary<SortedTwoIntegers, int[]> temp_edge_to_polygon = new Dictionary<SortedTwoIntegers, int[]>(new TwoIntegersComparer());

        print("Initializing...");
        // Initialize polygon list and edge_to_polygon dicit using the initial triangles.
        int cur_polygon = 0;
        for (int i = 0; i < in_BasicMeshData.triangles.Length; i += 3)
        {
            int v0 = in_BasicMeshData.triangles[i];
            int v1 = in_BasicMeshData.triangles[i + 1];
            int v2 = in_BasicMeshData.triangles[i + 2];

            Sc_Polygon newPolyObj = new Sc_Polygon(cur_polygon, new int[] { v0, v1, v2 }, new int[] { v0, v1, v2 });
            newPolyObj.calculateMeanStatistics(in_BasicMeshData.vertices);
            temp_indexed_polygons_dict.Add(cur_polygon, newPolyObj);

            SortedTwoIntegers[] triangle_edges = new SortedTwoIntegers[] { new SortedTwoIntegers(v0, v1), new SortedTwoIntegers(v0, v2), new SortedTwoIntegers(v1, v2) };
            foreach (SortedTwoIntegers sti_edge in triangle_edges)
            {
                if (temp_edge_to_polygon.ContainsKey(sti_edge))
                {
                    Debug.Assert(temp_edge_to_polygon[sti_edge][1] == -1, "Error: Edge already has two polygons!");
                    temp_edge_to_polygon[sti_edge][1] = cur_polygon;
                }
                else
                {
                    //print(string.Format("E{0} is not in e2t, adding to it.", sti_edge));
                    temp_edge_to_polygon.Add(sti_edge, new int[] { cur_polygon, -1 });
                }
            }
            ++cur_polygon;
        }

        print("Merging...");
        // Merge by each edge until no merges are posssible 
        bool merge_possible = true;
        List<SortedTwoIntegers> edges_removed = new List<SortedTwoIntegers>();
        int count_edges_removed = 0;
        while (merge_possible)
        {
            // set state as false for next iteration
            merge_possible = false;

            // Select a mergable edge from all the edges
            //List<SortedTwoIntegers> shuffled_edge_list = new List<SortedTwoIntegers>(temp_edge_to_polygon.Keys);
            //Sc_Utilities.Shuffle(shuffled_edge_list);
            //foreach (SortedTwoIntegers edge in shuffled_edge_list)
            foreach(var kvp in temp_edge_to_polygon)
            {
                SortedTwoIntegers edge = kvp.Key;

                int[] polygonIdentifiers = temp_edge_to_polygon[edge];
                Sc_Polygon pA = temp_indexed_polygons_dict[polygonIdentifiers[0]];
                Sc_Polygon pB = temp_indexed_polygons_dict[polygonIdentifiers[1]];

                Debug.Assert(pA.identifier != -1, string.Format("Error: PolygonID A is {1} on Edge{0}", edge, pA.identifier));
                Debug.Assert(pB.identifier != -1, string.Format("Error: PolygonID B is {1} on Edge{0}", edge, pB.identifier));
                Debug.Assert(pA.identifier != pB.identifier, string.Format("Error: Same Polygon {1}, {2} found on Edge{0}", edge, pA.identifier, pB.identifier));

                if (Mathf.Abs(pA.meanHeightSqrd - pB.meanHeightSqrd) < navHeightDiff && Sc_Polygon.isPolygonMergeConvex(pA, pB, edge, in_BasicMeshData.vertices))
                {
                    Sc_Polygon pCombined = Sc_Polygon.getMergedPolygon(cur_polygon, pA, pB, edge, in_BasicMeshData.vertices);

                    // set old identifiers for the polygon which represent the old small polygons that created this polygon
                    pCombined.oldIdentifiers.UnionWith(pA.oldIdentifiers);
                    pCombined.oldIdentifiers.UnionWith(pB.oldIdentifiers);
                    pCombined.oldIdentifiers.Add(pA.identifier);
                    pCombined.oldIdentifiers.Add(pB.identifier);

                    // replace old index references with the current new polygon
                    foreach (int oldId in pCombined.oldIdentifiers)
                    {
                        temp_indexed_polygons_dict.Remove(oldId);
                        temp_indexed_polygons_dict.Add(oldId, pCombined);
                    }

                    // mark pA and pB as old references
                    temp_old_polygons.Add(pA.identifier);
                    temp_old_polygons.Add(pB.identifier);

                    // add the current new polygon to the index
                    temp_indexed_polygons_dict.Add(cur_polygon, pCombined);

                    // clear the old pA and pB polygons as they are not of use anymore
                    pA.Clear();
                    pB.Clear();

                    ++cur_polygon;

                    merge_possible = true;

                    edges_removed.Add(edge);

                    ++count_edges_removed;

                    break;
                }
            }

            // remove the merged edges
            foreach (SortedTwoIntegers e in edges_removed)
            {
                temp_edge_to_polygon.Remove(e);
            }
            edges_removed.Clear();
        }

        print(string.Format("Removed {0} Edges.", count_edges_removed));
        print(string.Format("Old Polygons Count : {0}", temp_old_polygons.Count));
        print(string.Format("All Polygons Count : {0}", cur_polygon));
        print(string.Format("Edges Remaining : {0}", temp_edge_to_polygon.Count));

        print("Generating...");

        // Generate graph between polygons
        Dictionary<int, Dictionary<int, float>> navMeshGraph = new Dictionary<int, Dictionary<int, float>>();
        Dictionary<int, Sc_NavMeshConvexPolygon> navMeshNodes = new Dictionary<int, Sc_NavMeshConvexPolygon>();

        GameObject graphgo = refreshGameObject("Graph");

        foreach (var kvp in temp_edge_to_polygon)
        {
            Sc_Polygon pA = temp_indexed_polygons_dict[kvp.Value[0]];
            Sc_Polygon pB = temp_indexed_polygons_dict[kvp.Value[1]];
            float distAB = Mathf.Acos( Vector3.Dot(pA.center.normalized, pB.center.normalized) );

            if (!navMeshGraph.ContainsKey(pA.identifier))
            {
                navMeshGraph.Add(pA.identifier, new Dictionary<int, float>());
                Sc_Utilities.createPrimitiveGameObject(PrimitiveType.Sphere, "Node:" + pA.identifier, pA.center.normalized * pA.center.magnitude * 1.1f, Vector3.one * 2.0f, graphgo.transform);
            }
            if (!navMeshGraph.ContainsKey(pB.identifier))
            {
                navMeshGraph.Add(pB.identifier, new Dictionary<int, float>());
                Sc_Utilities.createPrimitiveGameObject(PrimitiveType.Sphere, "Node:" + pB.identifier, pB.center.normalized * pB.center.magnitude * 1.1f, Vector3.one * 2.0f, graphgo.transform);
            }

            if (!navMeshGraph[pA.identifier].ContainsKey(pB.identifier)) 
            {
                navMeshGraph[pA.identifier].Add(pB.identifier, distAB);
                Sc_Utilities.createLineGameObject(
                    string.Format("Edge_{0}_{1}", pA.identifier, pB.identifier), 
                    pA.center.normalized * pA.center.magnitude * 1.1f, 
                    pB.center.normalized * pB.center.magnitude * 1.1f, graphgo.transform);
            }

            if (!navMeshGraph[pB.identifier].ContainsKey(pA.identifier)) 
            { 
                navMeshGraph[pB.identifier].Add(pA.identifier, distAB); 
            }

        }

        // Create navmesh child if it doesn't exist
        string naveMeshName = "navMesh";
        Transform navMeshTransform = transform.Find(naveMeshName);
        GameObject navMeshGO;
        if (navMeshTransform == null)
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

        int poly_count = 0;
        foreach (KeyValuePair<int, Sc_Polygon> kvp in temp_indexed_polygons_dict)
        {
            if (temp_old_polygons.Contains(kvp.Key))
            {
                continue;
            }

            Mesh m = new Mesh();
            m.Clear();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = in_BasicMeshData.vertices;
            m.normals = in_BasicMeshData.normals;
            m.SetUVs(0, in_BasicMeshData.uvs);
            m.triangles = kvp.Value.trianglesList;

            var go = new GameObject(string.Format("M_i{0}_p{1}", poly_count, kvp.Key));
            go.transform.parent = navMeshGO.transform;
            go.layer = 9; // set NavMesh layer
            go.tag = "NavMesh";
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = m;
            Sc_NavMeshConvexPolygon nmcp = go.AddComponent<Sc_NavMeshConvexPolygon>();
            nmcp.mIdentifier = kvp.Value.identifier;
            nmcp.mNormalizedCenter = kvp.Value.center.normalized;
            nmcp.mGeoCenter = Sc_SphericalCoord.FromCartesian(kvp.Value.center).ToGeographic();
            navMeshNodes.Add(nmcp.mIdentifier, nmcp);

            // cleanup polygon
            kvp.Value.oldIdentifiers.Clear();
            kvp.Value.pointList.Clear();

            ++poly_count;
        }

        print(string.Format("New Polygons Count : {0}", poly_count));

        Sc_NavMesh navMeshObj = new Sc_NavMesh(navMeshGraph, navMeshNodes);

        return navMeshObj;
    }


    void OnValidate()
    {

    }
}
