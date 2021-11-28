using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_BuildingPlacement : MonoBehaviour
{
    RaycastHit hit;
    Vector3 movePoint;
    public GameObject CreatedBuildingPrefab;
    public GameObject ValidObject;
    public GameObject InvalidObject;
    public float CollisionRadius = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            transform.position = hit.point;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            transform.position = hit.point;
            Vector3 hit_point = hit.point;
            float hit_planet_radius = hit.transform.parent.gameObject.GetComponent<Sc_Planet>().planetRadius;
            Vector3 hit_planet_position = hit.transform.position;
            Vector3 hit_planet_normal = (hit_point - hit_planet_position).normalized;
            float rotAngle = Mathf.Acos( Vector3.Dot(Vector3.up, hit_planet_normal) );
            Vector3 rotAxis = Vector3.Cross(hit_point.normalized, Vector3.up).normalized;
            transform.rotation = Quaternion.AngleAxis( -rotAngle * Mathf.Rad2Deg,  rotAxis);

            if(Physics.SphereCast(new Ray(ray.origin, ray.direction), CollisionRadius, out hit, 50000.0f, (1 << 7)))
            {
                ValidObject.SetActive(false);
                InvalidObject.SetActive(true);
            }
            else
            {
                ValidObject.SetActive(true);
                InvalidObject.SetActive(false);

                if (Input.GetMouseButton(0))
                {
                    Instantiate(CreatedBuildingPrefab, transform.position, transform.rotation);
                    Destroy(gameObject);
                }
            }
        }

    }

}
