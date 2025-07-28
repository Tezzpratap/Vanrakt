using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public Transform cameraTransform;
    public Vector3 firstPersonOffset = new Vector3(0, 1.7f, 0.2f);
    public Vector3 thirdPersonOffset = new Vector3(0, 2f, -2f);

    private Rigidbody rb;
    private Animator animator;
    private float xRotation = 0f;
    private bool isGrounded = true;
    private bool isRunning = false;
    private bool isCrouching = false;
    private float originalHeight;
    private CapsuleCollider capsule;
    private bool isThirdPerson = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        originalHeight = capsule.height;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player left/right (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // Clamp for natural look
        cameraTransform.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        // Traversal input
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        isCrouching = Input.GetKey(KeyCode.C);

        float currentSpeed = walkSpeed;
        if (isRunning) currentSpeed = runSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;

        // Crouch collider adjustment
        if (isCrouching)
            capsule.height = originalHeight / 2f;
        else
            capsule.height = originalHeight;

        // Movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Always move relative to the player's Y rotation only
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 move = (right * moveX + forward * moveZ).normalized * currentSpeed;

        Vector3 rbVelocity = rb.velocity;
        rb.velocity = new Vector3(move.x, rbVelocity.y, move.z);

        // Animation
        animator.SetFloat("Speed", new Vector2(moveX, moveZ).magnitude * currentSpeed);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsCrouching", isCrouching);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("IsJumping", true);
            isGrounded = false;
        }

        //Debug
        if (Input.GetKeyDown(KeyCode.C))
            Debug.Log("Crouch key pressed");
        if (Input.GetButtonDown("Jump"))
            Debug.Log("Jump key pressed");
        if (Input.GetKeyDown(KeyCode.V))
        {
            isThirdPerson = !isThirdPerson;
        }

        // Respawn if out of play zone
        CheckRespawn();

        // After handling mouse look, update camera position:
        if (isThirdPerson)
            cameraTransform.localPosition = thirdPersonOffset;
        else
            cameraTransform.localPosition = firstPersonOffset;
    }

    private void CheckRespawn()
    {
        Vector3 pos = transform.position;
        if (pos.x < -10f || pos.x > 1000f || pos.z < -10f || pos.z > 1000f || pos.y < -10f)
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        transform.position = new Vector3(500f, 50f, 500f); // y=2 to avoid spawning inside ground
        rb.velocity = Vector3.zero;
        if (animator != null)
        {
            animator.SetBool("IsJumping", false);
        }
        isGrounded = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsJumping", false);
        }
    }
}
