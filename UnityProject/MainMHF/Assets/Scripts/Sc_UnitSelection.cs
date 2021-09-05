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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray2, out hit, 50000.0f, (1 << 6)))
            {
                GameObject go = hit.collider.gameObject;
                AddToSelection(go);
            }
            else
            {
                RemoveAllFromSelection();
            }
        }

    }
}
