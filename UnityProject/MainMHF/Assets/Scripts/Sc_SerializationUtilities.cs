using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerializationUtilities
{
    [Serializable]
    public struct SerializedVector3
    {
        public float x, y, z;

        public SerializedVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static explicit operator Vector3(SerializedVector3 sv)
        {
            return new Vector3(sv.x, sv.y, sv.z);
        }

        public static explicit operator SerializedVector3(Vector3 v)
        {
            return new SerializedVector3(v);
        }
    }

    [Serializable]
    public struct SerializedNavMeshConvexPolygon
    {
        public int mIdentifier;
        public SerializedVector3 mCenter;
        public SerializedVector3 mNormalizedCenter;
        public Sc_GeographicCoord mGeoCenter;

        public SerializedNavMeshConvexPolygon(Sc_NavMeshConvexPolygon v)
        {
            mIdentifier = v.mIdentifier;
            mCenter = (SerializedVector3)v.mCenter;
            mNormalizedCenter = (SerializedVector3)v.mNormalizedCenter;
            mGeoCenter = v.mGeoCenter;
        }

        public static explicit operator SerializedNavMeshConvexPolygon(Sc_NavMeshConvexPolygon v)
        {
            return new SerializedNavMeshConvexPolygon(v);
        }

        public static explicit operator Sc_NavMeshConvexPolygon(SerializedNavMeshConvexPolygon v)
        {
            var nmcp = new Sc_NavMeshConvexPolygon();
            nmcp.mIdentifier = v.mIdentifier;
            nmcp.mCenter = (Vector3)v.mCenter;
            nmcp.mNormalizedCenter = (Vector3)v.mNormalizedCenter;
            nmcp.mGeoCenter = v.mGeoCenter;
            return nmcp;
        }
    }

    [Serializable]
    public struct SerializedNavMesh
    {
        public Dictionary<int, Dictionary<int, float>> navMeshGraph;
        public Dictionary<int, SerializedNavMeshConvexPolygon> navMeshNodes;

        public SerializedNavMesh(Sc_NavMesh nm)
        {
            navMeshGraph = nm.navMeshGraph;
            navMeshNodes = new Dictionary<int, SerializedNavMeshConvexPolygon>();
            foreach (KeyValuePair<int, Sc_NavMeshConvexPolygon> kvp in nm.navMeshNodes)
            {
                navMeshNodes.Add(kvp.Key, (SerializedNavMeshConvexPolygon)kvp.Value);
            }
        }

        public static explicit operator Sc_NavMesh(SerializedNavMesh v)
        {
            Dictionary<int, Sc_NavMeshConvexPolygon> navMeshNodes = new Dictionary<int, Sc_NavMeshConvexPolygon>();
            foreach (var kvp in v.navMeshNodes)
            {
                navMeshNodes.Add(kvp.Key, (Sc_NavMeshConvexPolygon)kvp.Value);
            }

            return new Sc_NavMesh(v.navMeshGraph, navMeshNodes);
        }
    }

    [Serializable]
    public struct SerializedPolygon
    {
        public List<int> pointList;
        public int identifier;
        public int[] trianglesList;
        public HashSet<int> oldIdentifiers;
        public float meanHeightSqrd;
        public SerializedVector3 center;
        public float slopeAngleDegrees;
        public float lowPointHeight;
        public float highPointHeight;

        public SerializedPolygon(Sc_Polygon v)
        {
            pointList = v.pointList;
            identifier = v.identifier;
            trianglesList = v.trianglesList;
            oldIdentifiers = v.oldIdentifiers;
            meanHeightSqrd = v.meanHeightSqrd;
            center = (SerializedVector3)v.center;
            slopeAngleDegrees = v.slopeAngleDegrees;
            lowPointHeight = v.lowPointHeight;
            highPointHeight = v.highPointHeight;
        }

        public static explicit operator Sc_Polygon(SerializedPolygon v)
        {
            var p = new Sc_Polygon(v.identifier, v.pointList, v.trianglesList);

            p.pointList = v.pointList;
            p.identifier = v.identifier;
            p.trianglesList = v.trianglesList;
            p.oldIdentifiers = v.oldIdentifiers;
            p.meanHeightSqrd = v.meanHeightSqrd;
            p.center = (Vector3)v.center;
            p.slopeAngleDegrees = v.slopeAngleDegrees;
            p.lowPointHeight = v.lowPointHeight;
            p.highPointHeight = v.highPointHeight;

            return p;
        }
    }

    [Serializable]
    public struct SerializedNavMeshPolygons
    {
        public Dictionary<int, SerializedPolygon> indexed_polygons_list;

        public SerializedNavMeshPolygons(Dictionary<int, Sc_Polygon> v)
        {
            indexed_polygons_list = new Dictionary<int, SerializedPolygon>();
            foreach(var kvp in v)
            {
                indexed_polygons_list.Add(kvp.Key, new SerializedPolygon(kvp.Value));
            }
        }

        internal Dictionary<int, Sc_Polygon> GetConvertedDict()
        {
            var retval = new Dictionary<int, Sc_Polygon>();
            foreach (var kvp in indexed_polygons_list)
            {
                retval.Add(kvp.Key, (Sc_Polygon)kvp.Value);
            };
            return retval;
        }
    }
}