using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] public float baseSpeed = 8f;

    private float desiredMovedSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public bool isSliding;
    public float groundDrag;

    // [Header("Sliding")] public float maxSlideTime;
    // public float slideForce;
    // public float slideTimer;
    //
    // public float slideYScale;
    // public float originalYScale;

    [Header("Jump")] public float jumpForce;
    public float airMultiplier;
    private float lastSpeedBeforeTakeoff;


    [Header("Ground Check")] public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")] public float maxSlopAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Transform References")] public Transform orientation;

    // Displacement Calculation
    Vector3 lastPosition;
    [HideInInspector] public Vector3 displacement;

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
        HandleDrag();
        GroundCheck();
        playerUI.UpdateText("Velocity: " + rb.linearVelocity.magnitude.ToString("F2"));
    }

    public void ProcessMovement(Vector2 moveInput)
    {
        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (isSliding) return;

        if (moveDirection == Vector3.zero)
        {
            Debug.Log("No movement");
            // Dampen speed fast
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        float horizontalSpeed = new Vector3(displacement.x, 0, displacement.y).magnitude;
        float speedToApply = Mathf.Max(baseSpeed, horizontalSpeed);

        if (!isGrounded) speedToApply = lastSpeedBeforeTakeoff;
        if (speedToApply > baseSpeed) speedToApply *= isGrounded ? 0.985f : 0.99f;


        // The new velocity to apply
        Vector3 newVelocity = moveDirection.normalized * speedToApply;
        newVelocity.y = rb.linearVelocity.y; // Keep the current vertical speed

        if (isGrounded) rb.linearVelocity = newVelocity;
        else rb.AddForce(moveDirection.normalized * speedToApply, ForceMode.Force);

        if (!isGrounded && horizontalSpeed > baseSpeed)
        {
            Vector3 newHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            newHorizontalVelocity *= 0.98f;
            rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        }

        #region MyRegion

        // moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        //
        // if (OnSlope() && !exitingSlope)
        // {
        //     Debug.Log(OnSlope());
        //     rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);
        //
        //     if (rb.linearVelocity.y > 0)
        //     {
        //         rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        //     }
        // }
        // rb.useGravity = !OnSlope();

        #endregion
    }

    public void Jump()
    {
        exitingSlope = true;
        if (!isGrounded) return;

        lastSpeedBeforeTakeoff = displacement.magnitude;

        rb.linearVelocity += Vector3.up * jumpForce;
        // rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // private void GroundMovement()
    // {
    //     rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.VelocityChange);
    // }

    private void AirMovement()
    {
        rb.AddForce(moveDirection.normalized * airMultiplier, ForceMode.VelocityChange);
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