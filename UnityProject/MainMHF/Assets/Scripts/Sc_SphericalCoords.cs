using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_SphericalCoords
{
    [Tooltip("Radial distance")]
    public float r;

    [Tooltip("Polar angle")]
    public float theta;

    [Tooltip("Azimuthal angle")]
    public float phi;

    public override string ToString()
    {
        return "SC(" + r + ", " + theta + ", " + phi + ")";
    }

    public static Sc_SphericalCoords getSphericalCoordinates(Vector3 cartesian)
    {
        Sc_SphericalCoords ret = new Sc_SphericalCoords();

        if (cartesian.x == 0)
        {
            cartesian.x = Mathf.Epsilon;
        }

        ret.r = Mathf.Sqrt(
            Mathf.Pow(cartesian.x, 2) +
            Mathf.Pow(cartesian.y, 2) +
            Mathf.Pow(cartesian.z, 2)
        );

        ret.theta = Mathf.Acos(cartesian.y / ret.r);

        // use atan2 for built-in checks
        ret.phi = Mathf.Atan2(cartesian.z, cartesian.x);

        return ret;
    }

    public static Vector3 getCartesianCoordinates(Sc_SphericalCoords spherical)
    {
        Vector3 ret = new Vector3();

        ret.x = spherical.r * Mathf.Sin(spherical.theta) * Mathf.Cos(spherical.phi);
        ret.z = spherical.r * Mathf.Sin(spherical.theta) * Mathf.Sin(spherical.phi);

        ret.y = spherical.r * Mathf.Cos(spherical.theta);

        return ret;
    }
}
