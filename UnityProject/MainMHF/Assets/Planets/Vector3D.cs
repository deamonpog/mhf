using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vector3D
{
    public double x;
    public double y;
    public double z;

    public Vector3D(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 toVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}

public class Vector3DComparer : IEqualityComparer<Vector3D> 
{ 
    public bool Equals(Vector3D a, Vector3D b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public int GetHashCode(Vector3D obj)
    {
        return (int)(obj.x * 1.0 + obj.y * 100.0 + obj.z * 1000.0);
    }
}

public class Vector3Comparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 a, Vector3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public int GetHashCode(Vector3 obj)
    {
        return (int)(obj.x * 1.0 + obj.y * 100.0 + obj.z * 1000.0);
    }
}