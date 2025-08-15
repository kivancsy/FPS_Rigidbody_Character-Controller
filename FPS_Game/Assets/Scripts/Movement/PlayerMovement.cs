using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed;
    public float walkSpeed;
    public float slideSpeed;
    public float backpedalSpeed = 0.6f;

    private float desiredMovedSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public bool sliding;
    public bool wallRunning;

    public float groundDrag;

    // [Header("Sliding")] public float maxSlideTime;
    // public float slideForce;
    // public float slideTimer;
    //
    // public float slideYScale;
    // public float originalYScale;

    [Header("Jump")] public float jumpForce;
    public float airMultiplier;


    [Header("Ground Check")] public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")] public float maxSlopAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Transform References")] public Transform orientation;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private PlayerUI playerUI;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerUI = GetComponent<PlayerUI>();
    }

    private void Start()
    {
    }

    void Update()
    {
        SpeedControl();
        HandleDrag();
        GroundCheck();
        playerUI.UpdateText("Velocity: " + rb.linearVelocity.magnitude.ToString("F2"));
    }

    public void ProcessMovement(Vector2 moveInput)
    {
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if (OnSlope() && !exitingSlope)
        {
            Debug.Log(OnSlope());
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (isGrounded)
            GroundMovement();

        else if (!isGrounded)
            AirMovement();

        rb.useGravity = !OnSlope();
    }

    public void Jump()
    {
        exitingSlope = true;
        if (!isGrounded) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void GroundMovement()
    {
        rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.VelocityChange);
    }

    private void AirMovement()
    {
        rb.AddForce(moveDirection.normalized * airMultiplier, ForceMode.VelocityChange);
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        if (isGrounded)
            exitingSlope = false;
    }

    public bool OnSlope()
    {
        Vector3 rayStart = transform.position;
        float rayLength = playerHeight * 0.5f + 0.3f;

        if (Physics.Raycast(rayStart, Vector3.down, out slopeHit, rayLength, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            Debug.DrawLine(rayStart, slopeHit.point, Color.green);

            Debug.DrawRay(slopeHit.point, slopeHit.normal, Color.blue);
            return angle < maxSlopAngle && angle != 0;
        }

        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red);

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 inputDirection)
    {
        return Vector3.ProjectOnPlane(inputDirection, slopeHit.normal).normalized;
    }

    private void HandleDrag()
    {
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.down * playerHeight, Color.green);
    }
}