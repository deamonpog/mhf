using SerializationUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_NavMesh
{
    public Dictionary<int, Dictionary<int, float>> navMeshGraph;
    public Dictionary<int, Sc_NavMeshConvexPolygon> navMeshNodes;

    public Sc_NavMesh(Dictionary<int, Dictionary<int, float>> in_Graph, Dictionary<int, Sc_NavMeshConvexPolygon> in_Nodes)
    {
        navMeshGraph = in_Graph;
        navMeshNodes = in_Nodes;
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

    /// <summary>
    /// Returns the path needed to travese from start node to dest node
    /// </summary>
    /// <param name="in_StartNode">Starting polygon node index</param>
    /// <param name="in_DestNode">Destination polygon node index</param>
    /// <returns>List of nodees in the order that they need to be visited to get to destination, excluding start node</returns>
    public List<int> FindPathNaive(int in_StartNode, int in_DestNode)
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
    public List<int> FindPathAStar(int in_StartNode, int in_DestNode)
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
