using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float _mouseSensitivity = 100f;

    private float _xRotationCamera = 0f;
    private float _yRotationCamera = 0f;
    
    private float _speed = 0.1f;
    private float _scale = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right*_speed;
        }
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right*_speed;
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward*_speed;
        }
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward*_speed;
        }
        
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        _xRotationCamera += mouseX;
        _yRotationCamera -= mouseY;
        transform.localRotation = Quaternion.Euler(_yRotationCamera, _xRotationCamera, 0f);
        
        _speed += Input.mouseScrollDelta.y * _scale;
    }
}
