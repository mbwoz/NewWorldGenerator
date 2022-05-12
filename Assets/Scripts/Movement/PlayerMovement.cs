using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigidBody;
    private CapsuleCollider capsuleCollider;
    
    // player consts
    private float scale = 0.5f;

    // camera movement parameters
    private float _mouseSensitivity = 100f;
    private float _scrollSensitivity = 10f;
    private float xRotationCamera = 0f;
    
    // keyboard movement parameters
    private float _acceleration = 0.5f;
    private float _maxNaturalSpeed = 5f;
    
    // jump and grounding related parameters
    private float _yeet = 1000f;
    private float _maxJumps = 3;
    private float _scale = 0.5f;
    private float _jumpCooldownLength = 0.2f;
    // in seconds
    private int jumpCounter;
    private bool jumpCooldown = false;
    
    // compute shaders for starting position
    private int numThreads = 16;
    private int mult = 8;
    private int spread = (1 << 11);

    public ComputeShader surroundCS;
    private int kernelIndex;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer surroundingsBuffer;


    void Awake() {
        kernelIndex = surroundCS.FindKernel("Surround");
        positionsBuffer = new ComputeBuffer(mult * numThreads, sizeof(int) * 3);
        surroundingsBuffer = new ComputeBuffer(mult * numThreads, sizeof(int));
    }

    void Start() {
        transform.localScale = Vector3.one * scale;
        transform.position = GetStartingPosition();

        if (gameObject.GetComponent<Rigidbody>() == null) {
            gameObject.AddComponent<Rigidbody>();
        }
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.mass = 1f;
        rigidBody.drag = 3f;
        rigidBody.angularDrag = 0.05f;
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (gameObject.GetComponent<CapsuleCollider>() == null) {
            gameObject.AddComponent<CapsuleCollider>();
        }
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.center = Vector3.zero;
        capsuleCollider.radius = 0.5f;
        capsuleCollider.height = 2f;
        // 1 corresponds to Y axis
        capsuleCollider.direction = 1;
        capsuleCollider.material.staticFriction = 0;
        capsuleCollider.material.dynamicFriction = 0;
    }

    void OnDisable() {
        surroundingsBuffer.Release();
        positionsBuffer.Release();
    }

    // Update is called once per frame
    void Update() {
        HandleAdjustments();
        HandleLook();
    }
    // Independent of frame rate of the game
    void FixedUpdate() {
        HandleWalk();
        HandleJump();
    }
    
    private void HandleAdjustments() {
        // Debug.Log(Input.mouseScrollDelta.y);
        _mouseSensitivity = Mathf.Max(_mouseSensitivity + Input.mouseScrollDelta.y * _scrollSensitivity, 0.1f);
    }

    private void HandleLook() {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        xRotationCamera += mouseX;
        transform.localRotation = Quaternion.Euler(0f, xRotationCamera, 0f);
    }
    
    private void HandleWalk() {
        Vector3 inputVector = GetKeyboardInputVector();
        Vector3 flatVelocity = rigidBody.velocity;
        flatVelocity.y = 0;
        if (inputVector != Vector3.zero) {
            float maxLengthNew = Mathf.Max(flatVelocity.magnitude, _maxNaturalSpeed);
            Vector3 newSpeed = Vector3.ClampMagnitude(flatVelocity + inputVector * _acceleration, maxLengthNew);
            rigidBody.AddForce(newSpeed - flatVelocity, ForceMode.VelocityChange);
        }
        else {
            // Friction to decelerate when there are no inputs
            // Does not affect gravity
            rigidBody.AddForce(-flatVelocity, ForceMode.VelocityChange);
        }
    }
    
    private void HandleJump() {
        if (jumpCooldown) return;
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.F)) return;
        if (IsGrounded()) {
            jumpCounter = 0;
        } else {
            jumpCounter += 1;
        }
        if (jumpCounter > _maxJumps) {
            return;
        }
        rigidBody.AddForce(transform.up * _yeet);
        jumpCooldown = true;
        IEnumerator endCooldownCoroutine() {
            yield return new WaitForSeconds(_jumpCooldownLength);
            jumpCooldown = false;
        }
        StartCoroutine(endCooldownCoroutine());
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

    private Vector3 GetStartingPosition() {
        Vector3Int[] positions = new Vector3Int[mult * numThreads];
        int[] surroundings = new int[mult * numThreads];

        while (true) {
            for (int i = 0; i < positions.Length; i += 2) {
                positions[i].x = Mathf.RoundToInt((Random.value - 0.5f) * spread);
                positions[i].y = Mathf.RoundToInt((Random.value - 0.5f) * spread);
                positions[i].z = Mathf.RoundToInt((Random.value - 0.5f) * spread);
                positions[i+1] = positions[i];
                positions[i+1].y++;
            }

            surroundCS.SetBuffer(kernelIndex, "positions", positionsBuffer);
            positionsBuffer.SetData(positions);
            surroundCS.SetBuffer(kernelIndex, "surroundings", surroundingsBuffer);

            surroundCS.Dispatch(kernelIndex, mult, 1, 1);

            surroundingsBuffer.GetData(surroundings);

            for (int i = 0; i < surroundings.Length; i += 2) {
                if (surroundings[i] == 51 && surroundings[i+1] == 0) {
                    Vector3 position = positions[i+1];
                    position.x += 0.5f;
                    position.z += 0.5f;
                    return position;
                }
            }
        }
    }
}
