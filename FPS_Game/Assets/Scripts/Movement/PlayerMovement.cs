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

    [Header("Jump")] public float jumpForce;
    public float airMultiplier;


    [Header("Ground Check")] public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slop Handling")] public float maxSlopAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    public Transform orientation;

    private Vector3 moveDirection;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        SpeedControl();
        HandleDrag();
        GroundCheck();
    }

    public void ProcessMovement(Vector2 moveInput)
    {
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;


        if (isGrounded)
            GroundMovement();

        else if (!isGrounded)
            AirMovement();
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

    public void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    public void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
    }

    public bool OnSlope()
    {
        Vector3 rayStart = transform.position;
        float rayLength = playerHeight * 0.5f + 0.3f;

        if (Physics.Raycast(rayStart, Vector3.down, out slopeHit, rayLength, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            Debug.DrawLine(rayStart, slopeHit.point, Color.green);
            // Çarpma noktasından normali çiz (mavi)
            Debug.DrawRay(slopeHit.point, slopeHit.normal, Color.blue);
            return angle < maxSlopAngle && angle != 0;
        }

        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red);

        return false;
    }

    public void HandleDrag()
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