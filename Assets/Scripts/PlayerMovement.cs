using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigidBody;
    
    // camera movement parameters
    private float _mouseSensitivity = 100f;
    private float _xRotationCamera = 0f;
    
    // keyboard movement parameters
    private float _acceleration = 0.1f;
    private float _maxNaturalSpeed = 0.3f;
    
    //jump and grounding related parameters
    private float _yeet = 7f;
    private float _scale = 0.03f;
    
    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        HandleLook();
    }
    // Independent of frame rate of the game
    void FixedUpdate() {
        HandleWalk();
        HandleJump();
    }
    
    private void HandleLook() {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        _xRotationCamera += mouseX;
        transform.localRotation = Quaternion.Euler(0f, _xRotationCamera, 0f);
    }
    private void HandleWalk() {
        Vector3 inputVector = GetKeyboardInputVector();
        Vector3 flatVelocity = rigidBody.velocity;
        flatVelocity.y = 0;
        if (inputVector != Vector3.zero)
        {
            float maxLengthNew = Mathf.Max(flatVelocity.magnitude, _maxNaturalSpeed);
            Vector3 newSpeed = Vector3.ClampMagnitude(flatVelocity + inputVector * _acceleration, maxLengthNew);
            rigidBody.AddForce(newSpeed - flatVelocity, ForceMode.VelocityChange);
        }
        else
        {
            // Friction to decelerate when there are no inputs
            // Does not affect gravity
            rigidBody.AddForce(-flatVelocity, ForceMode.VelocityChange);
        }
    }
    private void HandleJump() {
        if(!IsGrounded())
        {
            return;
        }
        if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.F))
        {
            rigidBody.AddForce(transform.up * _yeet);
        }
    }
    
    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -transform.up, _scale+0.01f);
    }
    
    private Vector3 GetKeyboardInputVector() {
        Vector3 inputVector = new Vector3(0, 0, 0);
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            inputVector += transform.forward;
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            inputVector -= transform.forward;
        }
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            inputVector -= transform.right;
        }
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            inputVector += transform.right;
        }
        return inputVector;
    }
}
