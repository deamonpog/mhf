using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Sc_Utilities
{
    public enum PhysicsLayerMask
    {
        Units,
        Buildings,
        Ground,
        NavMesh
    }

    public static int GetPhysicsLayerMask(PhysicsLayerMask in_PLM)
    {
        switch (in_PLM)
        {
            case PhysicsLayerMask.Units:
                return (1 << 6);
            case PhysicsLayerMask.Buildings:
                return (1 << 7);
            case PhysicsLayerMask.Ground:
                return (1 << 8);
            case PhysicsLayerMask.NavMesh:
                return (1 << 9);
            default:
                return 0;
        }
    }

    public static GameObject createPrimitiveGameObject(PrimitiveType in_PrimitiveType, string in_GOName, Vector3 in_Position, Vector3 in_Scale, Transform in_Parent)
    {
        GameObject go = GameObject.CreatePrimitive(in_PrimitiveType);
        go.name = in_GOName;
        go.transform.position = in_Position;
        go.transform.localScale = in_Scale;
        go.transform.parent = in_Parent;
        return go;
    }

    public static GameObject createLineGameObject(string in_GOName, Vector3 in_Start, Vector3 in_End, Transform in_Parent)
    {
        GameObject go = new GameObject(in_GOName);
        go.transform.position = (in_Start + in_End) / 2.0f;
        go.transform.parent = in_Parent;
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.SetPosition(0, in_Start);
        lr.SetPosition(1, in_End);
        return go;
    }

    public static float AngularDistance(Vector3 normalized_A, Vector3 normalized_B)
    {
        return Mathf.Acos(Vector3.Dot(normalized_A, normalized_B));
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

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
        if (spherical.azimuthal < 0)
        {
            Debug.LogError("azimuth is too small");
        }
        if (360 < spherical.polar)
        {
            Debug.LogError("azimuth is too large");
        }
        return new Sc_GeographicCoord((Mathf.PI * 0.5f) - spherical.polar, spherical.azimuthal);
    }
}
