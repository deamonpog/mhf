using System.Collections.Generic;
using UnityEngine;

public partial class Sc_Planet
{
    class Sc_Polygon
    {
        public List<int> pointList;
        public int identifier;
        public int[] trianglesList;
        public HashSet<int> oldIdentifiers;
        public float meanHeightSqred;
        public Vector3 center;

        public Sc_Polygon(int in_identifier, List<int> in_vertIndexList, int[] in_trianglesList)
        {
            pointList = in_vertIndexList;
            identifier = in_identifier;
            trianglesList = in_trianglesList;
            oldIdentifiers = new HashSet<int>();
        }

        public Sc_Polygon(int in_identifier, int[] in_vertIndexList, int[] in_trianglesList)
        {
            pointList = new List<int>(in_vertIndexList);
            identifier = in_identifier;
            trianglesList = in_trianglesList;
            oldIdentifiers = new HashSet<int>();
        }

        public void Clear()
        {
            pointList.Clear();
            trianglesList = null;
            identifier = -1;
            oldIdentifiers.Clear();
        }

        public int Length => pointList.Count;

        public int getPoint(int currentIndex)
        {
            return pointList[currentIndex];
        }

        public int getNextPointIndex(int currentIndex)
        {
            return (currentIndex + 1) % pointList.Count;
        }

        public int getPreviousPointIndex(int currentIndex)
        {
            return (currentIndex - 1 + pointList.Count) % pointList.Count;
        }

        public int getIndexOfPoint(int currentPoint)
        {
            return pointList.IndexOf(currentPoint);
        }

        public float calculateMeanHeightSqred(Vector3[] indexedVertices)
        {
            //meanHeightSqred = 0.0f;
            for (int i = 0; i < trianglesList.Length; ++i)
            {
                center += indexedVertices[trianglesList[i]];
                //meanHeightSqred += indexedVertices[trianglesList[i]].sqrMagnitude;
            }
            //meanHeightSqred /= trianglesList.Length;
            center /= trianglesList.Length;
            meanHeightSqred = center.magnitude;
            return meanHeightSqred;
        }

        public float getAngleAtPoint(int currentPoint, Vector3[] indexedVertices)
        {
            // Let angle on Polygon A be PQR
            int pointidxQ = getIndexOfPoint(currentPoint);
            int pointidxP = getPreviousPointIndex(pointidxQ);
            int pointidxR = getNextPointIndex(pointidxQ);

            Vector3 P = indexedVertices[pointList[pointidxP]];
            Vector3 Q = indexedVertices[pointList[pointidxQ]];
            Vector3 R = indexedVertices[pointList[pointidxR]];

            float cosTheta = Vector3.Dot((P - Q).normalized, (R - Q).normalized);

            return Mathf.Acos(cosTheta);
        }

        public static bool isPolygonMergeConvex(Sc_Polygon pA, Sc_Polygon pB, SortedTwoIntegers edge, Vector3[] indexedVertices)
        {
            float angle_A_pA = pA.getAngleAtPoint(edge.A, indexedVertices);
            float angle_A_pB = pB.getAngleAtPoint(edge.A, indexedVertices);

            float angle_B_pA = pA.getAngleAtPoint(edge.B, indexedVertices);
            float angle_B_pB = pB.getAngleAtPoint(edge.B, indexedVertices);

            return ((angle_A_pA + angle_A_pB) <= Mathf.PI) && ((angle_B_pA + angle_B_pB) <= Mathf.PI);
        }

        public static Sc_Polygon getMergedPolygon(int identifier, Sc_Polygon pA, Sc_Polygon pB, SortedTwoIntegers edge, Vector3[] indexedVertices)
        {
            // Let PQR be the angle notation at edge
            // Therefore edge.A, edge.B are Q, R respectively
            // Find polygon with Q == edge.A and R == edge.B

            // Q,R in pA:
            int pointIdxQ_in_pA = pA.getIndexOfPoint(edge.A);
            int pointIdxR_in_pA = pA.getNextPointIndex(pointIdxQ_in_pA);
            // Q,R in pB:
            int pointIdxQ_in_pB = pB.getIndexOfPoint(edge.A);
            int pointIdxR_in_pB = pB.getNextPointIndex(pointIdxQ_in_pB);

            // Our condition is that Polygon poly0 should have Q == edge.A, R == edge.B 
            // Initially assumint poly0 is pA
            Sc_Polygon poly0 = pA;
            int pointIdxQ_poly0 = pointIdxQ_in_pA;
            int pointIdxR_poly0 = pointIdxR_in_pA;
            Sc_Polygon poly1 = pB;
            int pointIdxQ_poly1 = pointIdxQ_in_pB;
            if (pB.getPoint(pointIdxR_in_pB) == edge.B) // check if correct poly0 is pB
            {
                poly0 = pB;
                pointIdxQ_poly0 = pointIdxQ_in_pB;
                pointIdxR_poly0 = pointIdxR_in_pB;
                poly1 = pA;
                pointIdxQ_poly1 = pointIdxQ_in_pA;
            }

            int lastPointIdx_poly1 = poly1.getPreviousPointIndex(pointIdxQ_poly1);

            // Assert that edge exists and that it is used on different directions
            Debug.Assert(poly0.pointList[pointIdxQ_poly0] == edge.A && poly0.pointList[pointIdxR_poly0] == edge.B, "Error: poly0 does not have edge.");
            Debug.Assert((pA.pointList[pointIdxR_in_pA] == edge.B) ^ (pB.pointList[pointIdxR_in_pB] == edge.B), "Error: Polygons have the same direction for the edge.");
            Debug.Assert(poly1.pointList[pointIdxQ_poly1] == edge.A && poly1.pointList[lastPointIdx_poly1] == edge.B, "Error: poly1 doees not have reversed edge.");

            List<int> newPolygonList = new List<int>(poly0.Length + poly1.Length - 2);

            // start with poly0 edge.A
            newPolygonList.Add(poly0.getPoint(pointIdxQ_poly0));

            // append poly1 points starting from edge.A until we find edge.B
            int nextPointIdx_poly1 = poly1.getNextPointIndex(pointIdxQ_poly1);
            while (nextPointIdx_poly1 != lastPointIdx_poly1)
            {
                newPolygonList.Add(poly1.getPoint(nextPointIdx_poly1));
                nextPointIdx_poly1 = poly1.getNextPointIndex(nextPointIdx_poly1);
            }

            // add the edge.B
            newPolygonList.Add(poly0.getPoint(pointIdxR_poly0));

            // append poly0 points starting from the edge.B until we find edge.A
            int nextPointIdx_poly0 = poly0.getNextPointIndex(pointIdxR_poly0);
            while (nextPointIdx_poly0 != pointIdxQ_poly0)
            {
                newPolygonList.Add(poly0.getPoint(nextPointIdx_poly0));
                nextPointIdx_poly0 = poly0.getNextPointIndex(nextPointIdx_poly0);
            }

            int[] tris = new int[poly0.trianglesList.Length + poly1.trianglesList.Length];
            poly0.trianglesList.CopyTo(tris, 0);
            poly1.trianglesList.CopyTo(tris, poly0.trianglesList.Length);

            Sc_Polygon retval = new Sc_Polygon(identifier, newPolygonList, tris);
            retval.calculateMeanHeightSqred(indexedVertices);

            return retval;
        }
    }
}
