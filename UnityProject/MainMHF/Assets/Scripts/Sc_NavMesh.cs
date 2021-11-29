using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_NavMesh
{
    public Dictionary<int, Dictionary<int, float>> navMeshGraph;

    public Sc_NavMesh(Dictionary<int, Dictionary<int, float>> in_navMeshGraph)
    {
        navMeshGraph = in_navMeshGraph;
    }
}
