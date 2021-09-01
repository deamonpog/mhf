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
    public Sc_SphericalCoords destPosSC;
    public float thetaSpeed = 0f;
    public float phiSpeed = 0f;
    public float maxAccel = 10.0f;
    Vector2 accelThetaPhi = Vector2.zero;
    Vector2 velocityThetaPhi = Vector2.zero;
    float maxSpeedThetaPhi = 1.0f;

    Vector3 surfaceNormal;
    public float gizmoLen = 20.0f;
    RaycastHit hit;

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
                destPosSC = Sc_SphericalCoords.getSphericalCoordinates(destLocation);
            }
        }

        if (isMoving)
        {
            if(Vector3.Distance(destLocation, transform.position) < reachRadius)
            {
                isMoving = false;
            }
            else
            {
                Sc_SphericalCoords thisPosSC = Sc_SphericalCoords.getSphericalCoordinates(transform.position);
                accelThetaPhi = new Vector2(destPosSC.theta - thisPosSC.theta, destPosSC.phi - thisPosSC.phi);
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

                Sc_SphericalCoords nextPosSC = new Sc_SphericalCoords();
                nextPosSC.r = thisPosSC.r;
                nextPosSC.theta = thisPosSC.theta + velocityThetaPhi.x * Time.deltaTime;
                nextPosSC.phi = thisPosSC.phi + velocityThetaPhi.y * Time.deltaTime;

                Vector3 newPos = Sc_SphericalCoords.getCartesianCoordinates(nextPosSC);

                Vector3 planetSurfaceNormal = (newPos - mPlanet.transform.position).normalized; // surface normal at this newPos
                surfaceNormal = planetSurfaceNormal;

                transform.rotation = Quaternion.LookRotation(newPos - transform.position, transform.up);
                transform.position = newPos;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 10.0f);
        }

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
