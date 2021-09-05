using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Selectable : MonoBehaviour
{
    Color mOriginalColor;
    public bool isSelected;

    public void SetSelected(bool isSelected)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = (isSelected) ? Color.red : mOriginalColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        mOriginalColor = GetComponentInChildren<Renderer>().material.color;
        SetSelected(isSelected);
    }

    private void OnDestroy()
    {
        SetSelected(false);
    }
}
