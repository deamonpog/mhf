using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Unit : MonoBehaviour
{
    [Tooltip("The planet sphere object that this Unit resides on.")]
    public GameObject mPlanet;

    [Tooltip("Radius of the planet")]
    public float mPlanetRadius;

    public float mSpeed = 0.0f;
    public float mMaxSpeed = 100.0f;
    public float mMaxAccel = 50.0f;

    public bool mIsMoving = false;
    public float mUnitRadius = 5.0f;

    public Vector3 destLocation = new Vector3();
    Sc_GeographicCoord destGeo = new Sc_GeographicCoord();


    // debug stuff
    Vector3 surfaceNormal;
    public float gizmoLen = 2.0f;
    Sc_GeographicCoord thisGeo;
    
    public Vector3 altLoc = new Vector3();
    public Vector3 convertBack = new Vector3();
    public Sc_SphericalCoord altLocSC;
    public Sc_SphericalCoord convertBackSC;
    public Sc_GeographicCoord altLocGeo;
    RaycastHit hit, altHit;

    // Start is called before the first frame update
    void Start()
    {
        mPlanetRadius = mPlanet.GetComponent<Sc_PlanetDescriptor>().mRadius;

        // setup position and orientation
        Vector3 planetSurfaceNormal = (transform.position - mPlanet.transform.position).normalized;
        Quaternion initialOrientation = Quaternion.FromToRotation(transform.up, planetSurfaceNormal);

        transform.position = mPlanet.transform.position + mPlanetRadius * planetSurfaceNormal;
        transform.rotation = initialOrientation;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
            {
                mIsMoving = true;
                destLocation = hit.point;
                destGeo = Sc_SphericalCoord.FromCartesian(destLocation).ToGeographic();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray2, out altHit, 50000.0f, (1 << 8)))
            {
                altLoc = altHit.point;
                altLocSC = Sc_SphericalCoord.FromCartesian(altLoc);
                altLocGeo = altLocSC.ToGeographic();
                convertBackSC = altLocGeo.ToSpherical(altLocSC.radial);
                convertBack = convertBackSC.ToCartesian();
            }
        }

        if (mIsMoving)
        {
            if(Vector3.Distance(destLocation, transform.position) < mUnitRadius)
            {
                mIsMoving = false;
                mSpeed = 0f;
            }
            else
            {
                thisGeo = Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic();

                // find north at this point
                var scnorth = Sc_SphericalCoord.FromCartesian(transform.position);
                scnorth.polar -= 1.0f * Mathf.Deg2Rad;
                Vector3 north = scnorth.ToCartesian();

                // lon12: difference between longitudes between two points
                float lon12 = destGeo.lon - thisGeo.lon;

                // alpha1 : course (angle) at this start position
                float alpha1y = Mathf.Cos(destGeo.lat) * Mathf.Sin(lon12);
                float alpha1x = Mathf.Cos(thisGeo.lat) * Mathf.Sin(destGeo.lat) - Mathf.Sin(thisGeo.lat) * Mathf.Cos(destGeo.lat) * Mathf.Cos(lon12);
                float alpha1 = Mathf.Atan2(alpha1y, alpha1x);

                Quaternion qNorth = Quaternion.LookRotation(north - transform.position, transform.up);
                Quaternion qAlpha1 = Quaternion.Euler(0f, alpha1 * Mathf.Rad2Deg, 0f);
                Quaternion curRotation = qNorth * qAlpha1;

                mSpeed += mMaxAccel * Time.deltaTime;
                if(mSpeed > mMaxSpeed)
                {
                    mSpeed = mMaxSpeed;
                }

                Vector3 newPos = transform.position + (curRotation * Vector3.forward) * mSpeed * Time.deltaTime;
                var scnewpos = Sc_SphericalCoord.FromCartesian(newPos);
                scnewpos.radial = mPlanetRadius;
                newPos = scnewpos.ToCartesian();

                Quaternion newRotation = Quaternion.LookRotation(newPos - transform.position, transform.up);

                transform.position = newPos;
                transform.rotation = newRotation;

            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mIsMoving)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawSphere(destLocation, gizmoLen * 0.1f);

        //Gizmos.DrawSphere(altLoc, gizmoLen * 0.25f);
        Gizmos.color = Color.blue;
        //Gizmos.DrawCube(convertBack, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));

        var scnorth = Sc_SphericalCoord.FromCartesian(altLoc);
        scnorth.polar -= 1.0f * Mathf.Deg2Rad;
        Vector3 north = scnorth.ToCartesian();
        //Gizmos.DrawWireCube(north, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));
        //Gizmos.DrawLine(altLoc, altLoc + (north - altLoc).normalized * gizmoLen * 2.0f );


        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + surfaceNormal * gizmoLen * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoLen);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLen);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLen);
    }
}
