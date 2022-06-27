using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{
    public class Sc_UnitSelection : MonoBehaviour
    {
        public Dictionary<int, GameObject> mSelectedTable = new Dictionary<int, GameObject>();

        public float mSelectClickDuration;
        public bool mFirstSelectClick = false;
        public float mDeselectDuration = 0.1f;

        public void AddToSelection(GameObject go)
        {
            int id = go.GetInstanceID();
            if (!mSelectedTable.ContainsKey(id))
            {
                mSelectedTable.Add(id, go);
                go.GetComponent<Sc_Selectable>().SetSelected(true);
            }
        }

        public void RemoveFromSelection(int id)
        {
            GameObject go;
            if (mSelectedTable.TryGetValue(id, out go))
            {
                go.GetComponent<Sc_Selectable>().SetSelected(false);
                mSelectedTable.Remove(id);
            }
        }

        public void RemoveAllFromSelection()
        {
            foreach (KeyValuePair<int, GameObject> pair in mSelectedTable)
            {
                if (pair.Value != null)
                {
                    mSelectedTable[pair.Key].GetComponent<Sc_Selectable>().SetSelected(false);
                }
            }
            mSelectedTable.Clear();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetButtonDown("SelectClick"))
            {
                mSelectClickDuration = Time.time;

                if (Physics.Raycast(ray, out hit, 50000.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.Units)))
                {
                    GameObject go = hit.collider.gameObject;
                    AddToSelection(go);
                    mFirstSelectClick = mSelectedTable.Count < 1;
                }
            }

            if(Input.GetButtonUp("SelectClick"))
            {
                if((!mFirstSelectClick || !Input.GetButton("MoreSelect")) && Time.time - mSelectClickDuration < mDeselectDuration)
                {
                    RemoveAllFromSelection();
                }

                mFirstSelectClick = false;
            }

            if (Input.GetButtonDown("ActionClick"))
            {
                if (Physics.Raycast(ray, out hit, 50000.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.NavMesh)))
                {
                    NavMeshConvexPolygon nmcp = hit.collider.gameObject.GetComponent<NavMeshConvexPolygon>();
                    foreach (KeyValuePair<int, GameObject> pair in mSelectedTable)
                    {
                        if (pair.Value != null)
                        {
                            NavigatingAgentComponent unit = mSelectedTable[pair.Key].GetComponent<NavigatingAgentComponent>();
                            unit.mDestinationNavMeshNodeID = nmcp.mIdentifier;
                            unit.mIsMoving = true;
                            unit.mSpeed = 0f;
                            unit.mDestination = hit.point;
                            unit.mPathExist = false;
                            //unit.mDestinationGeo = Sc_SphericalCoord.FromCartesian(hit.point).ToGeographic();
                        }
                    }
                }

                if (Physics.Raycast(ray, out hit, 50000.0f, Sc_Utilities.GetPhysicsLayerMask(Sc_Utilities.PhysicsLayerMask.Units)))
                {
                    foreach (KeyValuePair<int, GameObject> pair in mSelectedTable)
                    {
                        if (pair.Value != null)
                        {
                            MissileLauncher unit = mSelectedTable[pair.Key].GetComponent<MissileLauncher>();
                            unit.mTarget = hit.point;
                            unit.mIsAttacking = true;
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawSphere(altLoc, gizmoLen * 0.25f);
            //Gizmos.color = Color.blue;
            //Gizmos.DrawCube(convertBack, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));

            //var scnorth = Sc_SphericalCoord.FromCartesian(altLoc);
            //scnorth.polar -= 1.0f * Mathf.Deg2Rad;
            //Vector3 north = scnorth.ToCartesian();
            //Gizmos.DrawWireCube(north, new Vector3(gizmoLen * 0.25f, gizmoLen * 0.25f, gizmoLen * 0.25f));
            //Gizmos.DrawLine(altLoc, altLoc + (north - altLoc).normalized * gizmoLen * 2.0f );

        }
    }
}