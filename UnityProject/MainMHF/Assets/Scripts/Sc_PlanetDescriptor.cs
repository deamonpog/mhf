using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_PlanetDescriptor : MonoBehaviour
{
    public float mRadius;

    // Start is called before the first frame update
    void Start()
    {
        mRadius = gameObject.transform.lossyScale.x / 10.0f;
        SphereCollider planetCollider = gameObject.GetComponent<SphereCollider>();
        if(planetCollider == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
