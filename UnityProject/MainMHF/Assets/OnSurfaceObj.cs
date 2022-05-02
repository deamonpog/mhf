using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{

    [Serializable]
    public class OnSurfaceObj : MonoBehaviour
    {
        public float mChangeLat;
        public float mChangeLon;
        public Sc_GeographicCoord mGeoCoord;
        public float mRadius = 500;

        public void changeGeoCoord(float lat, float lon)
        {
            mGeoCoord.lat += lat * Mathf.Deg2Rad;
            mGeoCoord.lon += lon * Mathf.Deg2Rad;
            transform.position = mGeoCoord.ToSpherical(mRadius).ToCartesian();
        }

        private void Awake()
        {
            mRadius = GameObject.FindGameObjectsWithTag("Planet")[0].GetComponent<PlanetComponent>().planetRadius;
        }

        public void placeOnSphere()
        {
            mGeoCoord = Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic();
            transform.position = mGeoCoord.ToSpherical(mRadius).ToCartesian();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}