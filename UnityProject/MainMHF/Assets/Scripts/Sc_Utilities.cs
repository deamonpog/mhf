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

    public static Vector3 GetClosestPointOnLineSegment(Vector3 pointOfInterest, Vector3 vA, Vector3 vB)
    {
        float cos_a = Vector3.Dot(pointOfInterest.normalized, vB.normalized);
        float cos_b = Vector3.Dot(vA.normalized, pointOfInterest.normalized);
        float cos_c = Vector3.Dot(vA.normalized, vB.normalized);

        float sin_a = Mathf.Acos(cos_a);
        float sin_b = Mathf.Acos(cos_b);
        float sin_c = Mathf.Acos(cos_c);

        float cos_A = (cos_a - cos_b * cos_c) / (sin_b * sin_c);
        float cos_B = (cos_b - cos_a * cos_c) / (sin_a * sin_c);

        //float cosalpha = Vector3.Dot((vA - vB).normalized, (pointOfInterest - vB).normalized);
        //float cosbeta = Vector3.Dot((vB - vA).normalized, (pointOfInterest - vA).normalized);

        if (cos_B <= 0)
        {
            return vB;
        }
        else if (cos_A <= 0)
        {
            return vA;
        }
        else
        {
            Debug.Log("Inside");
            //var lineDir = (vB - vA).normalized;
            //return vA + lineDir * Vector3.Dot((pointOfInterest - vA).normalized, lineDir);
            return Sc_SphericalCoord.FromCartesian((vA + vB) / 2).ToGeographic().ToSpherical(vA.magnitude).ToCartesian();
        }
    }

    public static bool isSTOnOppositeSidesOfOA(Vector3 OA, Vector3 OS, Vector3 OT)
    {
        return Vector3.Dot(Vector3.Cross(OA, OS), Vector3.Cross(OA, OT)) < 0;
    }

    public static Vector3 MiddlePointOfColinearSTU(Vector3 ST, Vector3 SU, Vector3 S, Vector3 T)
    {
        // STU is colinear and
        // S or T could be middle point
        // Therefore, TSU angle is either 0 or 180
        // Therefore, Cos(TSU) is either 1 or -1
        // Lets check this without troubling floatin points
        if (Vector3.Dot(ST, SU) > 0.1)
        {
            // Cos(TSU) must be 1, therefore TSU must be 0
            // Thus T is the middle point
            return T;
        }
        else
        {
            // Cos(TSU) must be -1, thus TSU must be 180
            // S is the middle point
            return S;
        }
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
            int k = UnityEngine.Random.Range(0, n + 1);
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
