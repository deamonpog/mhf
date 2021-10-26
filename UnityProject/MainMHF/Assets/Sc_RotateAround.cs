using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_RotateAround : MonoBehaviour
{
    public Vector3 rotDelta = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(rotDelta * Time.deltaTime);
    }
}
