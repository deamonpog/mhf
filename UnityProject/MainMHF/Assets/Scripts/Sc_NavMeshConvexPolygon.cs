using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_NavMeshConvexPolygon : MonoBehaviour
{
    public int mIdentifier;
    public Vector3 mNormalizedCenter;
    public Sc_GeographicCoord mGeoCenter;

    public Transform GetTransform()
    {
        return gameObject.transform;
    }
}
