using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{
    [System.Serializable]
    public class Sc_SphericalCoord
    {
        [Tooltip("Radial distance")]
        public float radial;

        [Tooltip("Polar angle / Colatitude range between 0 to PI. Polar = (PI / 2) - Latitude")]
        public float polar;

        [Tooltip("Azimuthal angle / Longitude  range between 0 to 2PI")]
        public float azimuthal;

        public Sc_SphericalCoord()
        {
            radial = 0;
            polar = 0;
            azimuthal = 0;
        }

        public Sc_SphericalCoord(float _radial, float _polar, float _azimuthal)
        {
            radial = _radial;
            polar = _polar;
            azimuthal = _azimuthal;
        }

        public override string ToString()
        {
            return "SC(" + radial + ", " + polar + ", " + azimuthal + ")";
        }

        public Vector3 ToCartesian()
        {
            return Sc_Utilities.getCartesianCoordinates(this);
        }

        public Sc_GeographicCoord ToGeographic()
        {
            return Sc_Utilities.getGeographicCoordinates(this);
        }

        public static Sc_SphericalCoord FromCartesian(Vector3 cartesian)
        {
            return Sc_Utilities.getSphericalCoordinates(cartesian);
        }

        public static Sc_SphericalCoord FromGeographic(float radius, Sc_GeographicCoord geo)
        {
            return Sc_Utilities.getSphericalCoordinates(radius, geo);
        }
    }
}