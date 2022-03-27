using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float _mouseSensitivity = 100f;
    private float _yRotationCamera = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        _yRotationCamera -= mouseY;
        _yRotationCamera = Mathf.Clamp(_yRotationCamera, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_yRotationCamera, 0f, 0f);
    }
}
