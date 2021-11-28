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

    public Vector3 mV3_Destination = new Vector3();
    public Sc_GeographicCoord mGeo_Destination = new Sc_GeographicCoord();

    public bool mIsAttacking = false;
    public Vector3 mTarget = new Vector3();
    public GameObject projectile;

    // debug stuff
    Vector3 surfaceNormal;
    public float gizmoLen = 2.0f;
    Sc_GeographicCoord thisGeo;

    // Start is called before the first frame update
    void Start()
    {
        mPlanetRadius = mPlanet.GetComponent<Sc_Planet>().planetRadius;

        // setup position and orientation
        Vector3 planetSurfaceNormal = (transform.position - mPlanet.transform.position).normalized;
        Quaternion initialOrientation = Quaternion.FromToRotation(transform.up, planetSurfaceNormal);

        transform.position = mPlanet.transform.position + mPlanetRadius * planetSurfaceNormal;
        transform.rotation = initialOrientation;
    }

    Quaternion getCourse(Sc_GeographicCoord in_destGeo)
    {
        thisGeo = Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic();

        // find north at this point
        var scnorth = Sc_SphericalCoord.FromCartesian(transform.position);
        scnorth.polar -= 1.0f * Mathf.Deg2Rad;
        Vector3 north = scnorth.ToCartesian();

        // lon12: difference between longitudes between two points
        float lon12 = in_destGeo.lon - thisGeo.lon;

        // alpha1 : course (angle) at this start position
        float alpha1y = Mathf.Cos(in_destGeo.lat) * Mathf.Sin(lon12);
        float alpha1x = Mathf.Cos(thisGeo.lat) * Mathf.Sin(in_destGeo.lat) - Mathf.Sin(thisGeo.lat) * Mathf.Cos(in_destGeo.lat) * Mathf.Cos(lon12);
        float alpha1 = Mathf.Atan2(alpha1y, alpha1x);

        Quaternion qNorth = Quaternion.LookRotation(north - transform.position, transform.up);
        Quaternion qAlpha1 = Quaternion.Euler(0f, alpha1 * Mathf.Rad2Deg, 0f);
        Quaternion curRotation = qNorth * qAlpha1;

        return curRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (mIsMoving)
        {
            thisGeo = Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic();
            if (Sc_GeographicCoord.AngularDistance(thisGeo, mGeo_Destination) < 0.01)
            {
                mIsMoving = false;
                mSpeed = 0f;
            }
            else
            {
                Quaternion curRotation = getCourse(mGeo_Destination);

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

        if (mIsAttacking)
        {
            Instantiate(projectile, transform.position, Quaternion.identity);
            Sc_Projectile proj = projectile.GetComponent<Sc_Projectile>();
            proj.mPlanet = this.mPlanet;
            proj.mPlanetRadius = this.mPlanetRadius;
            proj.mGravityAccel = -1.0f;
            Quaternion targetRot = getCourse(Sc_SphericalCoord.FromCartesian(mTarget).ToGeographic());
            proj.mVelocity = (targetRot * Vector3.forward) * 100.0f + (transform.position - mPlanet.transform.position).normalized * 10.0f;
            mIsAttacking = false;
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
        Gizmos.DrawSphere(mV3_Destination, gizmoLen * 0.1f);

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
