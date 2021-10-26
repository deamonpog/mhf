using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sc_GeographicCoord
{
    [Tooltip("Latitude range between -PI/2 to +PI/2. Latitude = (PI / 2) - Polar.")]
    public float lat;

    [Tooltip("Longitude / Azimuthal range between 0 to 2PI")]
    public float lon;

    public Sc_GeographicCoord()
    {
        lat = 0.0f;
        lon = 0.0f;
    }

    public Sc_GeographicCoord(float _lat, float _lon)
    {
        lat = _lat;
        lon = _lon;
    }

    public override string ToString()
    {
        return "Geo(" + lat + ", " + lon + ")";
    }

    public Sc_SphericalCoord ToSpherical(float radius)
    {
        return Sc_Utilities.getSphericalCoordinates(radius, this);
    }

    public static Sc_GeographicCoord FromSpherical(Sc_SphericalCoord spherical)
    {
        return Sc_Utilities.getGeographicCoordinates(spherical);
    }
}
