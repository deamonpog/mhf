using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Projectile : MonoBehaviour
{
    public Vector3 mVelocity = new Vector3();
    public float mGravityAccel = -100.0f;

    public GameObject mPlanet;
    public float mPlanetRadius;
    
    public float mCollisionRadius;

    public float mElasticity = 0.0f;
    public float mFriction = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        mCollisionRadius = gameObject.transform.lossyScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position + mVelocity * Time.deltaTime;
        Vector3 invGravityDir = (transform.position - mPlanet.transform.position).normalized;
        Vector3 newVelocity = mVelocity + invGravityDir * mGravityAccel * Time.deltaTime;
        if(Vector3.Distance(newPos, mPlanet.transform.position) < mPlanetRadius + mCollisionRadius)
        {
            newPos = (newPos - mPlanet.transform.position).normalized * (mPlanetRadius + mCollisionRadius);
            Vector3 invGravityComponent = Vector3.Dot(invGravityDir, newVelocity) * invGravityDir;
            Vector3 groundComponent = newVelocity - invGravityComponent;
            newVelocity = groundComponent * (1.0f - mFriction) - invGravityComponent * mElasticity;
        }

        transform.position = newPos;
        mVelocity = newVelocity;
    }
}
