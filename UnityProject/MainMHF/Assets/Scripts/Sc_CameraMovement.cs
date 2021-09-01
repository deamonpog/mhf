using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_CameraMovement : MonoBehaviour
{
    public GameObject mTargetPlanet;

    public float CameraMoveSpeed = 0.06f;
    public float CameraZoomSpeed = 20.0f;
    public float CameraRotateSpeed = 0.5f;

    Vector2 p1;
    Vector2 p2;

    public GameObject cameraGroundPos;
    public GameObject cameraArmPos;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = mTargetPlanet.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Zoom in-out towards the camera direction
        cameraArmPos.transform.position += cameraArmPos.transform.forward * CameraZoomSpeed * Input.GetAxis("Mouse ScrollWheel");

        // Rotate camera if MiddleMouseButton (MMB) is pressed. Move camera only if MMB button is not pressed.
        if (! Input.GetMouseButton(2))
        {
            // -- Mouse Move --
            Quaternion qh = Quaternion.Euler(0, 0, 0);
            Quaternion qv = Quaternion.Euler(0, 0, 0);
            // Horizontal Camera Move
            if (Input.mousePosition.x < 10)
            {
                qh = Quaternion.Euler(0, 0, CameraMoveSpeed);
            }
            else if (Input.mousePosition.x > Screen.width - 10)
            {
                qh = Quaternion.Euler(0, 0, -CameraMoveSpeed);
            }
            // Vertical Camera Move
            if (Input.mousePosition.y < 10)
            {
                qv = Quaternion.Euler(-CameraMoveSpeed, 0, 0);
            }
            else if (Input.mousePosition.y > Screen.height - 10)
            {
                qv = Quaternion.Euler(CameraMoveSpeed, 0, 0);
            }
            transform.rotation *=  qv * qh;
        }
        else
        {
            transform.rotation *= getCameraRotation();
        }

    }

    // Rotate camera around the focused position
    Quaternion getCameraRotation()
    {
        Quaternion retValue = Quaternion.identity;

        if (Input.GetMouseButtonDown(2))
        {
            p1 = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            p2 = Input.mousePosition;

            float dx = (p2 - p1).x * CameraRotateSpeed;
            float dy = (p2 - p1).y * CameraRotateSpeed;

            retValue = Quaternion.Euler(new Vector3(0, dx, 0));

            p1 = p2;
        }

        return retValue;
    }
}
