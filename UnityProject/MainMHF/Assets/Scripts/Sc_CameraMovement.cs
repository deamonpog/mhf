using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_CameraMovement : MonoBehaviour
{
    public float CameraMoveSpeed = 0.06f;
    public float CameraZoomSpeed = 20.0f;
    public float CameraRotateSpeed = 0.5f;

    float maxHeight = 40f;
    float minHeight = 4f;

    Vector2 p1;
    Vector2 p2;

    public GameObject cameraGroundPos;
    public GameObject cameraArmPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Zoom in-out towards the camera direction
        cameraArmPos.transform.position += cameraArmPos.transform.forward * CameraZoomSpeed * Input.GetAxis("Mouse ScrollWheel");

        if (! Input.GetMouseButton(2))
        {
            // -- Mouse Move --
            // Horizontal Camera Move
            if (Input.mousePosition.x < 10)
            {
                transform.rotation *= Quaternion.Euler(0, 0, CameraMoveSpeed);
            }
            else if (Input.mousePosition.x > Screen.width - 10)
            {
                transform.rotation *= Quaternion.Euler(0, 0, -CameraMoveSpeed);
            }
            // Vertical Camera Move
            if (Input.mousePosition.y < 10)
            {
                transform.rotation *= Quaternion.Euler(-CameraMoveSpeed, 0, 0);
            }
            else if (Input.mousePosition.y > Screen.height - 10)
            {
                transform.rotation *= Quaternion.Euler(CameraMoveSpeed, 0, 0);
            }
        }
        else
        {
            getCameraRotation();
        }
        
    }

    void getCameraRotation()
    {
        if (Input.GetMouseButtonDown(2))
        {
            p1 = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            p2 = Input.mousePosition;

            float dx = (p2 - p1).x * CameraRotateSpeed;
            float dy = (p2 - p1).y * CameraRotateSpeed;

            transform.rotation *= Quaternion.Euler(new Vector3(0, dx, 0));

            p1 = p2;
        }
    }
}
