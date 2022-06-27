using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{
    public class NavigatingAgentComponent : MonoBehaviour
    {
        [Tooltip("The planet sphere object that this Unit resides on.")]
        public GameObject mPlanet;

        [Tooltip("Radius of the planet")]
        public float mPlanetRadius;

        public float mSpeed = 0.0f;
        public float mMaxSpeed = 100.0f;
        public float mMaxAccel = 50.0f;

        public float mUnitRadius = 5.0f;

        public NavMesh mNavMesh;

        /// <summary>Set to true if needs to move.</summary>
        public bool mIsMoving = false;

        /// <summary>True if a path is generated for the current destination.</summary>
        public bool mPathExist = false;

        /// <summary>Destination point to travel as a Vector3.</summary>
        public Vector3 mDestination;

        /// <summary>Geographic coordinate of next waypoint on the current path</summary>
        public Sc_GeographicCoord mTargetGeo;

        /// <summary>
        /// List of vector3 positions that needs to be reached in the order to get to destination point.<br/>
        /// Includes the destination point.
        /// </summary>
        public List<Vector3> mPathToDestination;

        /// <summary>NavMesh node polygon that contains the destination point.</summary>
        public int mDestinationNavMeshNodeID = -1;

        /// <summary>NavMesh node polygon that contains the current location of this agent.</summary>
        public int mCurrentNavMeshNodeID = -1;

        // debug stuff
        //Vector3 surfaceNormal;
        //public float gizmoLen = 2.0f;
        //Sc_GeographicCoord thisGeo;

        private RaycastHit calculateSurfacePointHit(Vector3 in_NormalizedPosition)
        {
            RaycastHit hit;
            Vector3 org = in_NormalizedPosition * mPlanetRadius * 2.0f;
            Vector3 dir = (mPlanet.transform.position - org).normalized;
            bool hitTrue = Physics.Raycast(org, dir, out hit, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh));
            Debug.Assert(hitTrue, "Error: Unit did not identify the ground planet");
            return hit;
        }

        // Start is called before the first frame update
        void Start()
        {
            mPlanetRadius = mPlanet.GetComponent<PlanetComponent>().planetRadius;
            mNavMesh = mPlanet.GetComponent<PlanetComponent>().navMesh;

            RaycastHit hit = calculateSurfacePointHit((transform.position - mPlanet.transform.position).normalized);

            // setup position and orientation
            Quaternion initialOrientation = Quaternion.FromToRotation(transform.up, hit.normal);

            transform.position = hit.point;
            transform.rotation = initialOrientation;

            // find current navmeshnode
            findCurrentNavMesh();
        }

        

        private void findCurrentNavMesh()
        {
            RaycastHit hit;
            Vector3 org = transform.position.normalized * mPlanetRadius * 2.0f;
            Vector3 dir = (mPlanet.transform.position - org).normalized;
            bool hitTrue = Physics.Raycast(org, dir, out hit, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh));
            Debug.Assert(hitTrue, "Error: Unit did not identify the ground navMeshNode");
            mCurrentNavMeshNodeID = hit.collider.gameObject.GetComponent<NavMeshConvexPolygon>().mIdentifier;
        }

        void moveTowardsNextWaypoint()
        {
            // calculate surface speed
            mSpeed += mMaxAccel * Time.deltaTime;
            if (mSpeed > mMaxSpeed)
            {
                mSpeed = mMaxSpeed;
            }

            // calculate current slope
            Vector3 centerToCurPos = (transform.position - mPlanet.transform.position).normalized;
            RaycastHit hit_cur;
            Vector3 org_cur = centerToCurPos * mPlanetRadius * 2.0f;
            Vector3 dir_cur = (mPlanet.transform.position - org_cur).normalized;
            bool hitTrue_cur = Physics.Raycast(org_cur, dir_cur, out hit_cur, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh));
            Debug.Assert(hitTrue_cur, "Error: Unit did not identify the ground planet surface");

            float flatSpeed = Vector3.Dot(hit_cur.normal, centerToCurPos) * mSpeed;

            Quaternion curRotation = Sc_Utilities.GetCourse(transform, Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic(), mTargetGeo);
            Vector3 newPos = transform.position + (curRotation * Vector3.forward) * flatSpeed * Time.deltaTime;

            RaycastHit hit;
            Vector3 centerToNewPos = (newPos - mPlanet.transform.position).normalized;
            Vector3 org = centerToNewPos * mPlanetRadius * 2.0f;
            Vector3 dir = (mPlanet.transform.position - org).normalized;
            bool hitTrue = Physics.Raycast(org, dir, out hit, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh));
            Debug.Assert(hitTrue, "Error: Unit did not identify the ground planet surface");
            newPos = hit.point;

            Quaternion newRotation = Quaternion.LookRotation(newPos - transform.position, transform.up);
            transform.position = newPos;
            transform.rotation = newRotation;
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Assert(mNavMesh != null, $"{gameObject.name} : Nav Mesh is Null.");

            if (mIsMoving)
            {
                // calculate current navmesh node
                findCurrentNavMesh();

                // plan the path if needed a new plan.
                // we need a new plan in two occations if there is no path
                if (mPathExist == false)
                {
                    print("generating path");
                    mPathToDestination = mNavMesh.FindFunnelPathNaive(transform.position, mCurrentNavMeshNodeID, mDestination, mDestinationNavMeshNodeID);

                    print("path nodes count : " + mPathToDestination.Count);
                    /*for (int i = 0; i < mPathToDestination.Count; ++i)
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.localScale = new Vector3(9, 9, 9);
                        go.name = $"Path_{i}";
                        go.transform.position = mPathToDestination[i];
                    }*/

                    if (mPathToDestination.Count > 0)
                    {
                        mTargetGeo = Sc_SphericalCoord.FromCartesian(mPathToDestination[0]).ToGeographic();
                        mPathExist = true;
                    }
                    else
                    {
                        mIsMoving = false; // cannot move since there is no path
                    }
                }

                if (mPathToDestination.Count > 0)
                {
                    float arcDistance = mPlanetRadius * Sc_Utilities.AngularDistance(transform.position.normalized, mPathToDestination[0].normalized);
                    if (arcDistance < mUnitRadius)
                    {
                        mPathToDestination.RemoveAt(0);

                        if (mPathToDestination.Count > 0)
                        {
                            mTargetGeo = Sc_SphericalCoord.FromCartesian(mPathToDestination[0]).ToGeographic();
                            moveTowardsNextWaypoint();
                        }
                        else
                        {
                            // done
                            mIsMoving = false;
                            mPathExist = false;
                            mSpeed = 0f;
                        }
                    }
                    else
                    {
                        moveTowardsNextWaypoint();
                    }
                }
            }

        }

        private void OnDrawGizmos()
        {
            /*if (mIsMoving)
            {
                Gizmos.color = Color.red;
                for(int i=0; i < mPathToDestination.Count; ++i)
                {
                    Gizmos.DrawSphere(mPathToDestination[i], 5);
                }
            }
            else
            {
                Gizmos.color = Color.green;
            }*/
            /*
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(mDestination, 5);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + surfaceNormal * gizmoLen * 2f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoLen);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLen);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLen);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, mUnitRadius);
            */
        }
    }
}