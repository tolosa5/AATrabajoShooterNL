using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    float h;
    float v;
    float rotX;

    float sensibilidad = 400;

    [SerializeField] FpsController fpsController;

    Transform cuerpo;

    State state;

    enum State
    {
        Nomal,
        WallWalking
    }

    void Start()
    {
        cuerpo = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        switch(state)
        {
            default:
            case State.Nomal:
                CamNormal();
                break;

            case State.WallWalking:
                CamWall();
                break;

        }
    }

    void CamNormal()
    {
        h = Input.GetAxis("Mouse X");
        v = Input.GetAxis("Mouse Y");

        cuerpo.Rotate(new Vector3(0, h, 0) * sensibilidad * Time.deltaTime);

        rotX -= v * sensibilidad * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        if (fpsController.wall)
        {
            state = State.WallWalking;
        }
    }

    void CamWall()
    {
        v = Input.GetAxis("Mouse Y");


        rotX -= v * sensibilidad * Time.deltaTime;
        //rotX = Mathf.Clamp(rotX, -80f, 80f);

        transform.rotation = Quaternion.Euler(rotX, transform.eulerAngles.y, transform.eulerAngles.z);

        if (!fpsController.wall)
        {
            state = State.Nomal;
        }
    }
}
