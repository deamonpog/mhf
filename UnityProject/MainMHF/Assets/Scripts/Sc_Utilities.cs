using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Utilities
{
    public static Sc_SphericalCoord getSphericalCoordinates(Vector3 cartesian)
    {
        Sc_SphericalCoord ret = new Sc_SphericalCoord();

        if (cartesian.x == 0)
        {
            cartesian.x = Mathf.Epsilon;
        }

        ret.radial = Mathf.Sqrt(
            Mathf.Pow(cartesian.x, 2) +
            Mathf.Pow(cartesian.y, 2) +
            Mathf.Pow(cartesian.z, 2)
        );

        ret.polar = Mathf.Acos(cartesian.y / ret.radial);

        // use atan2 for built-in checks
        ret.azimuthal = Mathf.Atan2(cartesian.z, cartesian.x) + Mathf.PI;

        return ret;
    }

    public static Sc_SphericalCoord getSphericalCoordinates(float radius, Sc_GeographicCoord geo)
    {
        return new Sc_SphericalCoord(radius, (Mathf.PI * 0.5f) - geo.lat, geo.lon);
    }

    public static Vector3 getCartesianCoordinates(Sc_SphericalCoord spherical)
    {
        Vector3 ret = new Vector3();

        ret.x = spherical.radial * Mathf.Sin(spherical.polar) * Mathf.Cos(spherical.azimuthal - Mathf.PI);
        ret.z = spherical.radial * Mathf.Sin(spherical.polar) * Mathf.Sin(spherical.azimuthal - Mathf.PI);

        ret.y = spherical.radial * Mathf.Cos(spherical.polar);

        return ret;
    }

    public static Sc_GeographicCoord getGeographicCoordinates(Sc_SphericalCoord spherical)
    {
        if(spherical.polar < 0)
        {
            Debug.LogError("polar is negative");
        }
        if(180 < spherical.polar)
        {
            Debug.LogError("polar is too large");
        }
        if (spherical.azimuthal < -180)
        {
            Debug.LogError("azimuth is too small");
        }
        if (180 < spherical.polar)
        {
            Debug.LogError("azimuth is too large");
        }
        return new Sc_GeographicCoord((Mathf.PI * 0.5f) - spherical.polar, spherical.azimuthal);
    }
}
