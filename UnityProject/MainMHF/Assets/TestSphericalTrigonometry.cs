using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{
    public class TestSphericalTrigonometry : MonoBehaviour
    {
        public GameObject oA;
        public GameObject oB;
        public GameObject oC;

        public GameObject oI;

        public string dummy;

        private void OnValidate()
        {
            oA = InitObject("oA", Color.red);
            oB = InitObject("oB", Color.green);
            oC = InitObject("oC", Color.blue);

            oI = InitObject("oI", Color.magenta, PrimitiveType.Cube);

            var outPos = Sc_Utilities.GetClosestPointOnLineSegment(oC.transform.position.normalized, oA.transform.position.normalized, oB.transform.position.normalized);
            oI.transform.position = outPos;

            oA.GetComponent<OnSurfaceObj>().placeOnSphere();
            oB.GetComponent<OnSurfaceObj>().placeOnSphere();
            oC.GetComponent<OnSurfaceObj>().placeOnSphere();
            oI.GetComponent<OnSurfaceObj>().placeOnSphere();
        }

        private GameObject InitObject(string objName, Color col, PrimitiveType pt = PrimitiveType.Sphere)
        {
            var objTransform = transform.Find(objName);
            GameObject obj;
            if (objTransform == null)
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.name = objName;
                obj.AddComponent<OnSurfaceObj>();
                Material ma = new Material(Shader.Find("Standard"));
                ma.color = col;
                obj.GetComponent<Renderer>().material = ma;
                obj.transform.localScale = new Vector3(9, 9, 9);
                obj.transform.SetParent(transform);
            }
            else
            {
                obj = objTransform.gameObject;
            }
            return obj;
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