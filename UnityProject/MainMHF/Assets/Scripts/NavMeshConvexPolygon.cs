using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GalacticWar
{
    public class NavMeshConvexPolygon : MonoBehaviour
    {
        public int mIdentifier;
        public Vector3 mCenter;
        public Vector3 mNormalizedCenter;
        public Sc_GeographicCoord mGeoCenter;
        public bool mIsNavigable;

        public Transform GetTransform()
        {
            return gameObject.transform;
        }

        public void ChangeColor(Color c)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = c;
            gameObject.GetComponent<Renderer>().material = mat;
        }
    }
}