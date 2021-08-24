using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_StartBuilding : MonoBehaviour
{
    public GameObject TheBuildingType;

    public void spawnBuilding()
    {
        Instantiate(TheBuildingType);
    }
}
