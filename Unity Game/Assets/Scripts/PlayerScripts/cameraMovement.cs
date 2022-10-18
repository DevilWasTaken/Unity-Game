using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using pm = PlayerMovement;
using km = KeybindManager;

public class cameraMovement : MonoBehaviour
{
    public Transform orientation;
    float xRotation;
    float yRotation;
    float yRotationLock;
    public float sensX;
    public float sensY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (Input.GetKeyDown(km.crouchKey))
        {
            yRotationLock = yRotation;
        }
        if (pm.sliding)
        {
            yRotation = Mathf.Clamp(yRotation, yRotationLock - 130f, yRotationLock + 130f);
        }
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
