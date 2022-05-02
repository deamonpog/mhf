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

        /// <summary>A point on the edge betweeen current node polygon and next node polygon that we need to reach for transitioning to next node.</summary>
        Vector3 mNodeEdgePoint;

        public bool mIsAttacking = false;
        public Vector3 mTarget;

        public GameObject projectile;

        // debug stuff
        Vector3 surfaceNormal;
        public float gizmoLen = 2.0f;
        Sc_GeographicCoord thisGeo;

        void adjustHeight(Vector3 in_NormalizedPosition)
        {
            RaycastHit hit;
            Vector3 org = in_NormalizedPosition * mPlanetRadius * 2.0f;
            Vector3 dir = (mPlanet.transform.position - org).normalized;
            bool hitTrue = Physics.Raycast(org, dir, out hit, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.Ground));
            Debug.Assert(hitTrue, "Error: Unit did not identify the ground planet");
            transform.position = hit.point;
        }

        // Start is called before the first frame update
        void Start()
        {
            mPlanetRadius = mPlanet.GetComponent<PlanetComponent>().planetRadius;
            mNavMesh = mPlanet.GetComponent<PlanetComponent>().navMesh;

            // setup position and orientation
            Vector3 planetSurfaceNormal = (transform.position - mPlanet.transform.position).normalized;
            Quaternion initialOrientation = Quaternion.FromToRotation(transform.up, planetSurfaceNormal);

            //adjustHeight((mPlanet.transform.position + mPlanetRadius * planetSurfaceNormal).normalized);
            transform.position = mPlanet.transform.position + mPlanetRadius * planetSurfaceNormal;
            transform.rotation = initialOrientation;
            adjustHeight((transform.position - mPlanet.transform.position).normalized);

            // find current navmeshnode
            findCurrentNavMesh();
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

        void findCurrentNavMesh()
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

            Quaternion curRotation = getCourse(mTargetGeo);
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

        void moveTowardsGeoCoord(Sc_GeographicCoord in_GeoLocation)
        {
            Quaternion curRotation = getCourse(in_GeoLocation);

            mSpeed += mMaxAccel * Time.deltaTime;
            if (mSpeed > mMaxSpeed)
            {
                mSpeed = mMaxSpeed;
            }

            Vector3 newPos = transform.position + (curRotation * Vector3.forward) * mSpeed * Time.deltaTime;
            var scnewpos = Sc_SphericalCoord.FromCartesian(newPos);
            scnewpos.radial = mPlanetRadius;
            newPos = scnewpos.ToCartesian();

            RaycastHit hit;
            Vector3 org = newPos.normalized * mPlanetRadius * 2.0f;
            Vector3 dir = (mPlanet.transform.position - org).normalized;
            bool hitTrue = Physics.Raycast(org, dir, out hit, mPlanetRadius * 2.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh));
            Debug.Assert(hitTrue, "Error: Unit did not identify the ground planet surface");
            //scnewpos.radial = Vector3.Distance(hit.point, mPlanet.transform.position);
            //newPos = scnewpos.ToCartesian();

            Quaternion newRotation = Quaternion.LookRotation(newPos - transform.position, transform.up);

            transform.position = newPos;
            //adjustHeight(newPos.normalized);
            transform.rotation = newRotation;
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Assert(mNavMesh != null, $"{gameObject.name} : Nav Mesh is Null.");

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
                        //moveTowardsGeoCoord(mTargetGeo);
                        moveTowardsNextWaypoint();
                    }
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