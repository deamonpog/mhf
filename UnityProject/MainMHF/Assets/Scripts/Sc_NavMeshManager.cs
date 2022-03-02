using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_NavMeshManager : MonoBehaviour
{
    public Sc_NavMesh navMesh;
    public int nodeCount;

    // Start is called before the first frame update
    void Start()
    {
        print(navMesh);
        print("Nodes : " + nodeCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
