
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GalacticWar
{
    public class NavMesh
    {
        public Dictionary<int, Dictionary<int, float>> navMeshGraph;
        public Dictionary<int, NavMeshConvexPolygon> navMeshNodes;
        public Dictionary<SortedTwoIntegers, int[]> polygonsToEdge;
        public Vector3[] vertices;

        public NavMesh(Dictionary<int, Dictionary<int, float>> in_Graph, Dictionary<int, NavMeshConvexPolygon> in_Nodes, Dictionary<SortedTwoIntegers, int[]> in_polygonsToEdge, Vector3[] in_vertices)
        {
            navMeshGraph = in_Graph;
            navMeshNodes = in_Nodes;
            polygonsToEdge = in_polygonsToEdge;
            vertices = in_vertices;
        }

        public Vector3 GetClosestPointOnEdge(Vector3 curPoint, int currentPolygonID, int targetPolygonID)
        {
            int[] edge = polygonsToEdge[new SortedTwoIntegers(currentPolygonID, targetPolygonID)];

            Vector3 v0 = vertices[edge[0]];
            Vector3 v1 = vertices[edge[1]];
            return Sc_Utilities.GetClosestPointOnLineSegment(curPoint, v0, v1);
        }

        public float getHeuristicDistance(int in_A, int in_B)
        {
            return Mathf.Acos(Vector3.Dot(navMeshNodes[in_A].mNormalizedCenter, navMeshNodes[in_B].mNormalizedCenter));
        }

        public class IndexedOrderedValue : IComparable<IndexedOrderedValue>, IEquatable<IndexedOrderedValue>
        {
            public int index;
            public float value;

            public IndexedOrderedValue(int in_Index, float in_Value)
            {
                index = in_Index;
                value = in_Value;
            }

            public int CompareTo(IndexedOrderedValue obj)
            {
                return value.CompareTo(obj.value);
            }

            public bool Equals(IndexedOrderedValue other)
            {
                return index == other.index;
            }
        }

        public List<Vector3> FindPathNaive(Vector3 in_StartPoint, int in_StartNode, Vector3 in_DestPoint, int in_DestNode)
        {
            List<Vector3> path = new List<Vector3>();
            if (in_StartNode != in_DestNode)
            {
                var nodePath = FindNodePathNaive(in_StartNode, in_DestNode);

                // visual
                for (int i = 0; i < nodePath.Count; ++i)
                {
                    Debug.Log($"Path Node {i} : {nodePath[i]}");
                    Color c = (i % 2 == 0) ? Color.blue : Color.yellow;
                    //navMeshNodes[nodePath[i]].GetComponent<NavMeshConvexPolygon>().ChangeColor(c);
                }

                var curPos = in_StartPoint;
                for (int i = 1; i < nodePath.Count; ++i)
                {
                    var curNode = nodePath[i - 1];
                    var nextNode = nodePath[i];
                    curPos = GetClosestPointOnEdge(curPos, curNode, nextNode);
                    path.Add(curPos);
                }
            }
            else
            {
                int i = 0;
                Debug.Log($"Path Node {i} : {in_StartNode}");
                Color c = (i % 2 == 0) ? Color.blue : Color.yellow;
                //navMeshNodes[in_StartNode].GetComponent<NavMeshConvexPolygon>().ChangeColor(c);
            }
            path.Add(in_DestPoint);
            return path;
        }

        struct Diagonal
        {
            public Vector3 oldPointA;
            public Vector3 newPointB;

            public Diagonal(Vector3 oldPointA, Vector3 newPointB)
            {
                this.oldPointA = oldPointA;
                this.newPointB = newPointB;
            }

            public override string ToString()
            {
                return $"<{oldPointA},{newPointB}>";
            }
        }

        class Funnel
        {
            LinkedList<Vector3> _Deque;
            int _ApexIdx;
            LinkedListNode<Vector3> _ApexNode;

            public Funnel(LinkedList<Vector3> in_Deque, int in_ApexIdx)
            {
                _Deque = in_Deque;
                _ApexIdx = in_ApexIdx;

                _ApexNode = _Deque.First;
                int idx = 0;
                while (_ApexIdx != idx)
                {
                    _ApexNode = _ApexNode.Next;
                    ++idx;
                }
            }

            public Funnel(LinkedList<Vector3> in_Deque, int in_ApexIdx, LinkedListNode<Vector3> in_ApexNode)
            {
                _Deque = in_Deque;
                _ApexIdx = in_ApexIdx;
                _ApexNode = in_ApexNode;
            }

            public override string ToString()
            {
                string retval = "[";
                int i = 0;
                foreach (var val in _Deque)
                {
                    if (_ApexIdx == i)
                    {
                        retval = $"{retval} ({val}) ";
                    }
                    else
                    {
                        retval = $"{retval} {val} ";
                    }
                    ++i;
                }
                retval = $"{retval} ]";
                return retval;
            }

            private Funnel ConsumeDiagonal(Diagonal diag, ref Dictionary<Vector3, Vector3> ref_ParentOfPoint, Vector3 in_PlanetOrigin)
            {
                Debug.Assert(diag.oldPointA == _Deque.Last.Value || diag.oldPointA == _Deque.First.Value, "Error: old point not on the deque.");

                if (_Deque.Count < 3)
                {
                    Debug.Log($"Fresh funnel after hard turn");
                    Debug.Assert(_Deque.Count == 2, $"Error: dqueue size is {_Deque.Count}");
                    if (_ApexIdx == 0)
                    {
                        _Deque.AddFirst(diag.newPointB);
                        ++_ApexIdx;
                        ref_ParentOfPoint[diag.newPointB] = _ApexNode.Value;
                    }
                    else
                    {
                        _Deque.AddLast(diag.newPointB);
                        ref_ParentOfPoint[diag.newPointB] = _ApexNode.Value;
                    }
                    return this;
                }

                bool passedApexPoint = false;
                bool hasfoundWedge = false;

                LinkedListNode<Vector3> u = _Deque.First;
                LinkedListNode<Vector3> v = _Deque.First.Next;
                LinkedListNode<Vector3> w = _Deque.First.Next.Next;

                while (true)
                {
                    Vector3 normalS, normalB, normalT;
                    if (v == _ApexNode)
                    {
                        passedApexPoint = true;

                        normalS = Vector3.Cross(in_PlanetOrigin - v.Value, u.Value - v.Value).normalized;
                        normalB = Vector3.Cross(in_PlanetOrigin - v.Value, diag.newPointB - v.Value).normalized;
                        normalT = Vector3.Cross(in_PlanetOrigin - v.Value, w.Value - v.Value).normalized;
                    }
                    else if (!passedApexPoint)
                    {
                        normalS = Vector3.Cross(in_PlanetOrigin - v.Value, u.Value - v.Value).normalized;
                        normalB = Vector3.Cross(in_PlanetOrigin - v.Value, diag.newPointB - v.Value).normalized;
                        normalT = Vector3.Cross(in_PlanetOrigin - v.Value, v.Value - w.Value).normalized;
                    }
                    else
                    {
                        normalS = Vector3.Cross(in_PlanetOrigin - v.Value, v.Value - u.Value).normalized;
                        normalB = Vector3.Cross(in_PlanetOrigin - v.Value, diag.newPointB - v.Value).normalized;
                        normalT = Vector3.Cross(in_PlanetOrigin - v.Value, w.Value - v.Value).normalized;
                    }
                    float ST_angle = Sc_Utilities.AngularDistance(normalS, normalT);
                    float SB_angle = Sc_Utilities.AngularDistance(normalS, normalB);
                    float TB_angle = Sc_Utilities.AngularDistance(normalT, normalB);
                    Debug.Log($"STangle : {ST_angle * Mathf.Rad2Deg} , SBangle : {SB_angle * Mathf.Rad2Deg}, TBangle : {TB_angle * Mathf.Rad2Deg}");

                    if (SB_angle <= ST_angle && TB_angle <= ST_angle)
                    {
                        hasfoundWedge = true;
                        break;
                    }
                    else
                    {
                        if (w.Next == null)
                        {
                            Debug.LogWarning($"Breaking because of null neighbor after u:{u.Value}, v:{v.Value}, w:{w.Value}");
                            break;
                        }

                        u = u.Next;
                        v = v.Next;
                        w = w.Next;
                    }
                }

                bool oldPointA_is_front = diag.oldPointA == _Deque.First.Value;
                if (hasfoundWedge)
                {
                    Debug.Log("Wedge found");
                    ref_ParentOfPoint[diag.newPointB] = v.Value;

                    if (oldPointA_is_front)
                    {
                        // Funnel 1 (Front to v, new point B)
                        LinkedList<Vector3> newDeque_1 = new LinkedList<Vector3>();
                        LinkedListNode<Vector3> current_1 = _Deque.First;
                        int idx = 0;
                        while (current_1 != v)
                        {
                            newDeque_1.AddLast(current_1.Value);
                            current_1 = current_1.Next;
                            ++idx;
                        }
                        newDeque_1.AddLast(v.Value);
                        int idx_of_v_in_F1 = idx;
                        newDeque_1.AddLast(diag.newPointB);

                        Funnel F1 = new Funnel(newDeque_1, passedApexPoint ? _ApexIdx : idx_of_v_in_F1);
                        return F1;
                    }
                    else
                    {
                        // Funnel 2 (new point B, v to Back)
                        LinkedList<Vector3> newDeque_2 = new LinkedList<Vector3>();
                        newDeque_2.AddLast(diag.newPointB);

                        LinkedListNode<Vector3> current_2 = v;
                        int idx = 1;
                        int idx_of_apex_in_F2 = 1;
                        int idx_of_v_in_F2 = 1;
                        while (current_2 != _Deque.Last)
                        {
                            if (_ApexNode == current_2)
                            {
                                idx_of_apex_in_F2 = idx;
                            }
                            newDeque_2.AddLast(current_2.Value);
                            current_2 = current_2.Next;
                            ++idx;
                        }
                        newDeque_2.AddLast(_Deque.Last.Value);

                        Funnel F2 = new Funnel(newDeque_2, passedApexPoint ? idx_of_v_in_F2 : idx_of_apex_in_F2);
                        return F2;
                    }
                }
                else
                {
                    Vector3 normalS = Vector3.Cross(in_PlanetOrigin - _ApexNode.Value, _Deque.First.Value - _ApexNode.Value).normalized;
                    Vector3 normalB = Vector3.Cross(in_PlanetOrigin - _ApexNode.Value, diag.newPointB - _ApexNode.Value).normalized;
                    Vector3 normalT = Vector3.Cross(in_PlanetOrigin - _ApexNode.Value, _Deque.Last.Value - _ApexNode.Value).normalized;

                    if (Sc_Utilities.isSTOnOppositeSidesOfOA(normalS, normalT, normalB))
                    {
                        Debug.Log($"opposite sides -> S:{normalS}, B:{normalB}, T:{normalT}");
                        if (oldPointA_is_front)
                        {
                            Debug.Log($"hard turn at corner 0 {diag.oldPointA}");
                            // next diagonal newpoint will be added to the front
                            LinkedList<Vector3> newDeque_3 = new LinkedList<Vector3>(new Vector3[] { diag.oldPointA, diag.newPointB });
                            Funnel F3 = new Funnel(newDeque_3, 0);
                            ref_ParentOfPoint[diag.newPointB] = diag.oldPointA;
                            return F3;
                        }
                        else
                        {
                            Debug.Log("open mouth first side.");
                            ref_ParentOfPoint[diag.newPointB] = _Deque.First.Value;
                            _Deque.AddFirst(diag.newPointB);
                            ++_ApexIdx;
                            return this;
                        }
                    }
                    else
                    {
                        Debug.Log($"same side -> S:{normalS}, B:{normalB}, T:{normalT}");
                        if (oldPointA_is_front)
                        {
                            Debug.Log("open mouth last side.");
                            ref_ParentOfPoint[diag.newPointB] = _Deque.Last.Value;
                            _Deque.AddLast(diag.newPointB);
                            return this;
                        }
                        else
                        {
                            Debug.Log($"hard turn at corner 1 {diag.oldPointA}");
                            LinkedList<Vector3> newDeque_4 = new LinkedList<Vector3>(new Vector3[] { diag.newPointB, diag.oldPointA });
                            Funnel F4 = new Funnel(newDeque_4, 1);
                            // next diagonal newpoint will be added to the back
                            ref_ParentOfPoint[diag.newPointB] = diag.oldPointA;
                            return F4;
                        }
                    }
                }
            }

            public static void UpdateFunnel(ref Funnel ref_Funnel, Diagonal in_Diagonal, ref Dictionary<Vector3, Vector3> ref_ParentOfPoint, Vector3 in_PlanetOrigin)
            {
                ref_Funnel = ref_Funnel.ConsumeDiagonal(in_Diagonal, ref ref_ParentOfPoint, in_PlanetOrigin);
            }
        }

        public List<Vector3> FindFunnelPathNaive(Vector3 in_StartPoint, int in_StartNode, Vector3 in_DestPoint, int in_DestNode)
        {
            List<Vector3> returnPath = new List<Vector3>();

            if (in_StartNode != in_DestNode)
            {
                var nodePath = FindNodePathNaive(in_StartNode, in_DestNode);
                if (nodePath == null)
                {
                    return returnPath; // return empty path
                }
                Debug.Assert(nodePath.Count > 1, "Error: Path lenth is not greater than 1.");

                // visual
                for (int i = 0; i < nodePath.Count; ++i)
                {
                    Debug.Log($"Path Node {i} : {nodePath[i]}");
                    Color c = (i % 2 == 0) ? Color.blue : Color.yellow;
                    //navMeshNodes[nodePath[i]].GetComponent<NavMeshConvexPolygon>().ChangeColor(c);
                }

                var edge_AB = polygonsToEdge[new SortedTwoIntegers(nodePath[0], nodePath[1])];
                Vector3 vA = vertices[edge_AB[0]];
                Vector3 vB = vertices[edge_AB[1]];

                Dictionary<Vector3, Vector3> parentOfPoint = new Dictionary<Vector3, Vector3>();
                parentOfPoint[vA] = in_StartPoint;
                parentOfPoint[vB] = in_StartPoint;

                Funnel funnel = new Funnel(new LinkedList<Vector3>(new Vector3[] { vA, in_StartPoint, vB }), 1);

                Debug.Log($"1st Funnel : {funnel}");

                Diagonal lastDiagonal = new Diagonal(vA, vB);

                int fid = 0;
                for (int i = 2; i < nodePath.Count; ++i)
                {
                    // previous edge is AB
                    // current edge is CD
                    var edge_CD = polygonsToEdge[new SortedTwoIntegers(nodePath[i - 1], nodePath[i])];
                    Vector3 vC = vertices[edge_CD[0]];
                    Vector3 vD = vertices[edge_CD[1]];
                    List<Diagonal> diagonals = GetDiagonals(vA, vB, vC, vD);

                    foreach (Diagonal diag in diagonals)
                    {
                        Debug.Log($"Diag : {diag}");
                        Debug.Log($"Funnel {fid++} is {funnel}");
                        Funnel.UpdateFunnel(ref funnel, diag, ref parentOfPoint, Vector3.zero);
                        Debug.Log($"Result Funnel : {funnel}");

                        lastDiagonal = diag;
                    }

                    // pass current edge as the previous edge for the next iteration
                    vA = vC;
                    vB = vD;
                }

                Diagonal finalDiag = new Diagonal(lastDiagonal.newPointB, in_DestPoint);
                Debug.Log($"final Diag : {finalDiag}");
                Debug.Log($"Funnel {fid++} is {funnel}");
                Funnel.UpdateFunnel(ref funnel, finalDiag, ref parentOfPoint, Vector3.zero);
                Debug.Log($"final ({fid}) funnel : {funnel}");

                Debug.Log("Parents:");
                foreach (var kvp in parentOfPoint)
                {
                    Debug.Log($"{kvp.Key} : {kvp.Value}");
                }
                Debug.Log("====");

                Debug.Log("back track...");
                returnPath.Add(in_StartPoint);
                int detectInfiniteLoop = 500;
                var currentPoint = parentOfPoint[in_DestPoint];
                Debug.Log($"dest: {in_DestPoint}");
                Debug.Log($"cp: {currentPoint}");
                while (currentPoint != in_StartPoint && detectInfiniteLoop > 0)
                {
                    returnPath.Insert(1, currentPoint);
                    currentPoint = parentOfPoint[currentPoint];
                    Debug.Log($"cp: {currentPoint}");
                    --detectInfiniteLoop;
                }
            }
            else
            {
                // visual
                int i = 0;
                Debug.Log($"Path Node {i} : {in_StartNode}");
                Color c = (i % 2 == 0) ? Color.blue : Color.yellow;
                //navMeshNodes[in_StartNode].GetComponent<NavMeshConvexPolygon>().ChangeColor(c);
            }
            returnPath.Add(in_DestPoint);
            return returnPath;

            ///<summary>
            /// Function caller should guarentee that AB and CD are
            /// two different line segments.
            ///</summary>
            static List<Diagonal> GetDiagonals(Vector3 vA, Vector3 vB, Vector3 vC, Vector3 vD)
            {
                // step 1. Check for triangles
                if (vA == vC || vB == vC)
                {
                    // its already a triangle
                    return new List<Diagonal>(new Diagonal[] { new Diagonal(vC, vD) });
                }
                if (vA == vD || vB == vD)
                {
                    // its already a triangle
                    return new List<Diagonal>(new Diagonal[] { new Diagonal(vD, vC) });
                }

                // step 2. Check for all colinear case
                Vector3 AB = (vB - vA);
                Vector3 CD = (vD - vC);
                if (Vector3.Cross(AB, CD) == Vector3.zero)
                {
                    return GetDiagonalsFromRectPoints(vA, vB, vC, vD);
                }

                // step 3. Check for 3 colinear points
                //      We first pick one point to be left out
                Vector3 AC = (vC - vA);
                Vector3 BD = (vD - vB);
                // check for case 3.1 ABC is colinear and D is left out
                if (Vector3.Cross(AB, AC) == Vector3.zero)
                {
                    var v = Sc_Utilities.MiddlePointOfColinearSTU(AB, AC, vA, vB);
                    Diagonal d1 = new Diagonal(v, vD);
                    Diagonal d2 = new Diagonal(vD, vC);
                    return new List<Diagonal>(new Diagonal[] { d1, d2 });
                }
                // check for case 3.2 ABD is colinear and C is left out
                if (Vector3.Cross(AB, BD) == Vector3.zero)
                {
                    var v = Sc_Utilities.MiddlePointOfColinearSTU(-AB, BD, vB, vA);
                    Diagonal d1 = new Diagonal(v, vC);
                    Diagonal d2 = new Diagonal(vC, vD);
                    return new List<Diagonal>(new Diagonal[] { d1, d2 });
                }
                // check for case 3.3 ACD is colinear and B is left out
                if (Vector3.Cross(CD, AC) == Vector3.zero)
                {
                    var v = Sc_Utilities.MiddlePointOfColinearSTU(CD, -AC, vC, vD);
                    Diagonal d1 = new Diagonal(vB, v);
                    Diagonal d2 = new Diagonal(v, v == vC ? vD : vC);
                    return new List<Diagonal>(new Diagonal[] { d1, d2 });
                }
                // check for case 3.4 CDB is colinear and A is left out
                if (Vector3.Cross(CD, BD) == Vector3.zero)
                {
                    var v = Sc_Utilities.MiddlePointOfColinearSTU(-CD, -BD, vD, vC);
                    Diagonal d1 = new Diagonal(vA, v);
                    Diagonal d2 = new Diagonal(v, v == vC ? vD : vC);
                    return new List<Diagonal>(new Diagonal[] { d1, d2 });
                }

                return GetDiagonalsFromRectPoints(vA, vB, vC, vD);
            }

            static List<Diagonal> GetDiagonalsFromRectPoints(Vector3 vA, Vector3 vB, Vector3 vC, Vector3 vD)
            {
                Vector3 DB = (vB - vD).normalized;
                Vector3 DC = (vC - vD).normalized;
                Vector3 DA = (vA - vD).normalized;

                float BDC = Mathf.Acos(Vector3.Dot(DB, DC));
                float ADC = Mathf.Acos(Vector3.Dot(DA, DC));

                var d1 = new Diagonal(vA, vD);
                var d2 = new Diagonal(vD, vC);
                if (BDC < ADC)
                {
                    Vector3 AC = (vC - vA).normalized;
                    d1 = new Diagonal(vA, vC);
                    d2 = new Diagonal(vC, vD);
                }
                return new List<Diagonal>(new Diagonal[] { d1, d2 });
            }
        }

        /// <summary>
        /// Returns the path needed to travese from start node to dest node
        /// </summary>
        /// <param name="in_StartNode">Starting polygon node index</param>
        /// <param name="in_DestNode">Destination polygon node index</param>
        /// <returns>List of nodees in the order that they need to be visited to get to destination, including start node</returns>
        public List<int> FindNodePathNaive(int in_StartNode, int in_DestNode)
        {
            Dictionary<int, bool> node_visited = new Dictionary<int, bool>();
            Dictionary<int, float> node_distance = new Dictionary<int, float>();
            Dictionary<int, int> node_parent = new Dictionary<int, int>();
            foreach (int node in navMeshGraph.Keys)
            {
                node_visited[node] = false;
                node_distance[node] = Mathf.Infinity;
                node_parent[node] = node;
            }
            node_distance[in_StartNode] = 0;

            int currentNode = in_StartNode;

            while (currentNode != -1)
            {
                // process unvisited neighbors of current
                foreach (KeyValuePair<int, float> nbr_dist in navMeshGraph[currentNode])
                {
                    if (!node_visited[nbr_dist.Key])
                    {
                        float distViaCurrent = nbr_dist.Value + node_distance[currentNode];
                        if (distViaCurrent < node_distance[nbr_dist.Key])
                        {
                            node_distance[nbr_dist.Key] = distViaCurrent;
                            node_parent[nbr_dist.Key] = currentNode;
                        }
                    }
                }

                // mark as visited
                node_visited[currentNode] = true;

                // check goal found?
                if (node_visited[in_DestNode])
                {
                    // done
                    List<int> totalPath = new List<int>();
                    totalPath.Insert(0, in_DestNode);
                    int cur = in_DestNode;
                    while (node_parent.ContainsKey(cur))
                    {
                        cur = node_parent[cur];
                        totalPath.Insert(0, cur);
                        if (cur == in_StartNode)
                        {
                            break;
                        }
                    }
                    return totalPath;
                }

                // get smallest distance valued node 
                int minnode = -1;
                float minval = Mathf.Infinity;
                foreach (var node_dist in node_distance)
                {
                    if (!node_visited[node_dist.Key] && node_dist.Value < minval)
                    {
                        minval = node_dist.Value;
                        minnode = node_dist.Key;
                    }
                }

                currentNode = minnode;
            }

            if (currentNode == -1)
            {
                Debug.Log("There is no connecting path!!!");
                return null;
            }

            return null;
        }

        /// <summary>
        /// Returns the path needed to travese from start node to dest node
        /// </summary>
        /// <param name="in_StartNode">Starting polygon node index</param>
        /// <param name="in_DestNode">Destination polygon node index</param>
        /// <returns>List of nodees in the order that they need to be visited to get to destination, excluding start node</returns>
        public List<int> FindNodePathAStar(int in_StartNode, int in_DestNode)
        {
            if (in_StartNode == in_DestNode)
            {
                return new List<int>();
            }

            Dictionary<int, float> openSet_node_to_fscore = new Dictionary<int, float>();// node to fscore
            SortedDictionary<float, List<int>> openSet_fscore_to_nodes = new SortedDictionary<float, List<int>>(); // fscore to nodes

            Dictionary<int, int> cameFrom = new Dictionary<int, int>();
            Dictionary<int, float> gScore = new Dictionary<int, float>();
            Dictionary<int, float> fScore = new Dictionary<int, float>();
            foreach (int node in navMeshGraph.Keys)
            {
                gScore[node] = Mathf.Infinity;
                fScore[node] = Mathf.Infinity;
            }

            gScore[in_StartNode] = 0;
            fScore[in_StartNode] = getHeuristicDistance(in_StartNode, in_DestNode);

            openSet_node_to_fscore.Add(in_StartNode, fScore[in_StartNode]);
            openSet_fscore_to_nodes.Add(fScore[in_StartNode], new List<int>());
            openSet_fscore_to_nodes[fScore[in_StartNode]].Add(in_StartNode);



            while (openSet_node_to_fscore.Count > 0)
            {
                var enumerator = openSet_fscore_to_nodes.GetEnumerator();
                enumerator.MoveNext();
                var current = enumerator.Current.Value[0];
                if (current == in_DestNode)
                {
                    // done
                    List<int> totalPath = new List<int>();
                    int cur = in_DestNode;
                    while (cameFrom.ContainsKey(cur))
                    {
                        cur = cameFrom[cur];
                        totalPath.Insert(0, cur);
                    }
                    return totalPath;
                }

                foreach (KeyValuePair<int, float> nbr_fscore in navMeshGraph[current])
                {
                    int nbr = nbr_fscore.Key;
                    float tentative_gscore = gScore[current] + navMeshGraph[current][nbr];
                    if (tentative_gscore < gScore[nbr])
                    {
                        cameFrom[nbr] = current;
                        gScore[nbr] = tentative_gscore;
                        fScore[nbr] = tentative_gscore + getHeuristicDistance(nbr, in_DestNode);

                        if (openSet_node_to_fscore.ContainsKey(nbr))
                        {
                            float currentNbrFScore = openSet_node_to_fscore[nbr];
                            if (openSet_fscore_to_nodes.ContainsKey(currentNbrFScore))
                            {
                                openSet_fscore_to_nodes[currentNbrFScore].Remove(current);
                                if (openSet_fscore_to_nodes[currentNbrFScore].Count <= 0)
                                {
                                    openSet_fscore_to_nodes.Remove(currentNbrFScore);
                                }
                            }
                        }

                        float newNbrFScore = fScore[nbr];
                        if (openSet_fscore_to_nodes.ContainsKey(newNbrFScore))
                        {
                            openSet_fscore_to_nodes[newNbrFScore].Add(nbr);
                        }
                        else
                        {
                            openSet_fscore_to_nodes.Add(newNbrFScore, new List<int>());
                            openSet_fscore_to_nodes[newNbrFScore].Add(nbr);
                        }
                        openSet_node_to_fscore[nbr] = newNbrFScore;
                    }
                }
            }

            return null;
        }
    }
}