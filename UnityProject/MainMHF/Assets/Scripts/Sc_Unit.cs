using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Unit : MonoBehaviour
{
    public Collider CollisionBoundingVolume;

    public GameObject _Planet;
    
    float _PlanetRadius;

    float linearSpeed = 0.0f;

    public float mass = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        _PlanetRadius = _Planet.transform.lossyScale.x / 10.0f;
        Vector3 p_to_this = (transform.position - _Planet.transform.position).normalized;
        transform.position = _PlanetRadius * p_to_this;
        //transform.rotation = Quaternion.Euler( Vector3.Cross(transform.up, transform.position.normalized) );
    }

    // Update is called once per frame
    void Update()
    {
        //print(transform.up.normalized);

        Quaternion finalRotation = transform.rotation;

        Vector3 p_to_this = (transform.position - _Planet.transform.position).normalized;

        float forwardForceRatio = Input.GetAxis("Vertical");
        
        float accelerationForce = 10f * forwardForceRatio;

        float acceleration = accelerationForce / mass;

        linearSpeed += acceleration * Time.deltaTime;

        Vector3 newPosition = transform.position + transform.forward.normalized * linearSpeed * Time.deltaTime;

        RaycastHit hit;
        if( Physics.Raycast(transform.position, p_to_this * -1.0f, out hit, 5000.0f, (1 << 8)))
        {
            newPosition = hit.point;
        }

        transform.position = newPosition;

        Quaternion q = Quaternion.FromToRotation(Vector3.up, p_to_this);

        finalRotation = q;

        float turnRightForceRatio = Input.GetAxis("Horizontal");

        if (turnRightForceRatio > 0)
        {
            Quaternion qturn = Quaternion.AngleAxis(30f , Vector3.up);
            print("turning");
            finalRotation = q * qturn;
        }

        transform.rotation = finalRotation;

    }
}
