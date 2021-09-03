using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Unit : MonoBehaviour
{
    [Tooltip("The planet sphere object that this Unit resides on.")]
    public GameObject mPlanet; 

    protected float mPlanetRadius;

    public float mLinearSpeed = 0.0f;
    public float mTurnSpeed = 0.0f;

    public float mMaxTurnRate = 40.0f;
    [Tooltip("Max rate at which the pitch of the unit will change. Useful to match this with climbable angles.")]
    public float mMaxPitchRate = 10.0f;

    public float mMass = 1.0f;

    public bool isMoving = false;
    public float reachRadius = 5.0f;
    public Vector3 destLocation;
    public Sc_SphericalCoord destPosSC;
    public float thetaSpeed = 0f;
    public float phiSpeed = 0f;
    public float maxAccel = 10.0f;
    Vector2 accelThetaPhi = Vector2.zero;
    Vector2 velocityThetaPhi = Vector2.zero;
    float maxSpeedThetaPhi = 100.0f;

    Vector3 surfaceNormal;
    public float gizmoLen = 20.0f;
    public float distRatio = 0.1f;
    public Vector3 midpoint;
    public float destLat;
    public float destLon;
    Sc_GeographicCoord midGeo;
    Sc_GeographicCoord thisGeo;
    Sc_GeographicCoord destGeo = new Sc_GeographicCoord();
    public Vector3 altLoc = new Vector3();
    public Vector3 convertBack = new Vector3();
    public Sc_SphericalCoord altLocSC;
    public Sc_SphericalCoord convertBackSC;
    public Sc_GeographicCoord altLocGeo;
    RaycastHit hit, altHit;

    // Start is called before the first frame update
    void Start()
    {
        mPlanetRadius = mPlanet.transform.lossyScale.x / 10.0f;
        Vector3 p_to_this = (transform.position - mPlanet.transform.position).normalized;
        Quaternion initialOrientation = Quaternion.FromToRotation(transform.up, p_to_this);

        transform.position = mPlanet.transform.position + mPlanetRadius * p_to_this;
        transform.rotation = initialOrientation;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
            {
                isMoving = true;
                destLocation = hit.point;
                destPosSC = Sc_SphericalCoord.FromCartesian(destLocation);
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

        if (isMoving)
        {
            if(Vector3.Distance(destLocation, transform.position) < reachRadius)
            {
                isMoving = false;
                mLinearSpeed = 0f;
            }
            else
            {
                Sc_SphericalCoord thisPosSpherical = Sc_SphericalCoord.FromCartesian(transform.position);

                // Great circle navigation method

                thisGeo = thisPosSpherical.ToGeographic();
                destGeo = destPosSC.ToGeographic();
                destLat = Mathf.Rad2Deg * destGeo.lat;
                destLon = Mathf.Rad2Deg * destGeo.lon;

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

                mLinearSpeed += maxAccel * Time.deltaTime;
                if(mLinearSpeed > 200.0f)
                {
                    mLinearSpeed = 200.0f;
                }

                Vector3 newPos = transform.position + (curRotation * Vector3.forward) * mLinearSpeed * Time.deltaTime;
                var scnewpos = Sc_SphericalCoord.FromCartesian(newPos);
                scnewpos.radial = thisPosSpherical.radial;
                newPos = scnewpos.ToCartesian();

                Quaternion newRotation = Quaternion.LookRotation(newPos - transform.position, transform.up);

                transform.position = newPos;
                transform.rotation = newRotation;

                /*

                // alpha0 : course (angle) at node position (node is the position where course path crosses the equator)
                float alpha0y = Mathf.Sin(alpha1) * Mathf.Cos(thisGeo.lat);
                float alpha0x = Mathf.Sqrt( Mathf.Pow(Mathf.Cos(alpha1), 2) + Mathf.Pow(Mathf.Sin(alpha1), 2) * Mathf.Pow(Mathf.Sin(thisGeo.lat), 2));
                float alpha0 = Mathf.Atan2(alpha0y, alpha0x);

                // sigma12 : central angle between start and destination points (angular distance)
                float sigma12y = Mathf.Sqrt(Mathf.Pow(Mathf.Cos(thisGeo.lat) * Mathf.Sin(destGeo.lat) - Mathf.Sin(thisGeo.lat) * Mathf.Cos(destGeo.lat) * Mathf.Cos(lon12), 2) 
                    + Mathf.Pow(Mathf.Cos(destGeo.lat) * Mathf.Sin(lon12), 2));
                float sigma12x = Mathf.Sin(thisGeo.lat) * Mathf.Sin(destGeo.lat) + Mathf.Cos(thisGeo.lat) * Mathf.Cos(destGeo.lat) * Mathf.Cos(lon12);
                float sigma12 = Mathf.Atan2(sigma12y, sigma12x);

                // sigma01 : angular distance between start position and the node of the great circle
                float sigma01 = (thisGeo.lat == 0 && alpha1 == 90.0f) ? 0.0f : Mathf.Atan2(Mathf.Tan(thisGeo.lat), Mathf.Cos(alpha1));

                // sigma02 : angular distance between destination position and the node of the great circle
                float sigma02 = sigma01 + sigma12;

                // lon0 : longitude at the node of the great circle
                float lon01 = Mathf.Atan2(Mathf.Sin(alpha0) * Mathf.Sin(sigma01), Mathf.Cos(sigma01));
                float lon0 = thisGeo.lon - lon01;

                // desired point calculation
                float sigma = distRatio * (sigma01 + sigma02);

                float phi_y = Mathf.Cos(alpha0) * Mathf.Sin(sigma);
                float phi_x = Mathf.Sqrt(Mathf.Pow(Mathf.Cos(sigma),2) + Mathf.Pow(Mathf.Sin(alpha0), 2) * Mathf.Pow(Mathf.Sin(sigma), 2));
                float newlat_phi = Mathf.Atan2(phi_y, phi_x);

                float newlonDiff = Mathf.Atan2(Mathf.Sin(alpha0) * Mathf.Sin(sigma), Mathf.Cos(sigma));
                float newlon = newlonDiff + lon0;

                midGeo = new Sc_GeographicCoord(newlat_phi, newlon);
                midpoint = midGeo.ToSpherical(thisPosSpherical.radial).ToCartesian();
                */

                // Plannar trigonometry method

                /*
                accelThetaPhi = new Vector2(destPosSC.polar - thisPosSpherical.polar, destPosSC.azimuthal - thisPosSpherical.azimuthal);
                accelThetaPhi.Normalize();
                accelThetaPhi *= maxAccel;

                velocityThetaPhi += accelThetaPhi * Time.deltaTime;
                if(velocityThetaPhi.magnitude > maxSpeedThetaPhi)
                {
                    velocityThetaPhi.Normalize();
                    velocityThetaPhi *= maxSpeedThetaPhi;
                }
                if(accelThetaPhi.magnitude == 0f)
                {
                    velocityThetaPhi = Vector2.zero;
                }

                Sc_SphericalCoord nextPosSC = new Sc_SphericalCoord();
                nextPosSC.radial = thisPosSpherical.radial;
                nextPosSC.polar = thisPosSpherical.polar + velocityThetaPhi.x * Time.deltaTime;
                nextPosSC.azimuthal = thisPosSpherical.azimuthal + velocityThetaPhi.y * Time.deltaTime;

                Vector3 newPos = nextPosSC.ToCartesian();

                

                Vector3 planetSurfaceNormal = (newPos - mPlanet.transform.position).normalized; // surface normal at this newPos
                surfaceNormal = planetSurfaceNormal;

                //transform.rotation = Quaternion.LookRotation(newPos - transform.position, transform.up);
                
                transform.rotation = Quaternion.LookRotation(north - newPos, transform.up);
                transform.position = newPos;
                */
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(destLocation, gizmoLen * 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(midpoint, gizmoLen * 0.2f);

        Gizmos.DrawSphere(altLoc, gizmoLen * 0.25f);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(convertBack, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));

        var scnorth = Sc_SphericalCoord.FromCartesian(altLoc);
        scnorth.polar -= 1.0f * Mathf.Deg2Rad;
        Vector3 north = scnorth.ToCartesian();
        Gizmos.DrawWireCube(north, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));
        Gizmos.DrawLine(altLoc, altLoc + (north - altLoc).normalized * gizmoLen * 2.0f );


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
