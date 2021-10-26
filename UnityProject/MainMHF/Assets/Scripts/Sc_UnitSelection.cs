using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_UnitSelection : MonoBehaviour
{
    public Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();

    public void AddToSelection(GameObject go)
    {
        int id = go.GetInstanceID();
        if (!selectedTable.ContainsKey(id))
        {
            selectedTable.Add(id, go);
            go.GetComponent<Sc_Selectable>().SetSelected(true);
        }
    }

    public void RemoveFromSelection(int id)
    {
        GameObject go;
        if( selectedTable.TryGetValue(id, out go))
        {
            go.GetComponent<Sc_Selectable>().SetSelected(false);
            selectedTable.Remove(id);
        }
    }

    public void RemoveAllFromSelection()
    {
        foreach(KeyValuePair<int,GameObject> pair in selectedTable)
        {
            if(pair.Value != null)
            {
                selectedTable[pair.Key].GetComponent<Sc_Selectable>().SetSelected(false);
            }
        }
        selectedTable.Clear();
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
            if (!Input.GetButton("MoreSelect"))
            {
                RemoveAllFromSelection();
            }

            if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 6)))
            {
                GameObject go = hit.collider.gameObject;
                AddToSelection(go);
            }
        }

        if (Input.GetButtonDown("ActionClick"))
        {
            
            if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
            {
                foreach (KeyValuePair<int, GameObject> pair in selectedTable)
                {
                    if (pair.Value != null)
                    {
                        Sc_Unit unit = selectedTable[pair.Key].GetComponent<Sc_Unit>();
                        unit.mIsMoving = true;
                        unit.mSpeed = 0f;
                        unit.mV3_Destination = hit.point;
                        unit.mGeo_Destination = Sc_SphericalCoord.FromCartesian(hit.point).ToGeographic();
                    }
                }
            }

            if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 6)))
            {
                foreach (KeyValuePair<int, GameObject> pair in selectedTable)
                {
                    if (pair.Value != null)
                    {
                        Sc_Unit unit = selectedTable[pair.Key].GetComponent<Sc_Unit>();
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
