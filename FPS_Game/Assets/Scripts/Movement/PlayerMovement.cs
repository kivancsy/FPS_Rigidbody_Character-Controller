using DG.Tweening;
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

    [Header("Sliding")] public float slideSpeedThreshold = 6f; // Minimum speed needed to begin sliding

    public float
        addedSlideSpeed = 3f; // Adding flat speed + also add a percentage of horizontal speed up to 66% of base speed

    public float
        slideSpeedDampening = 0.99f; // The speed will be multiplied by this every frame (to stop in ~3 seconds)

    public float keepSlidingSpeedThreshold = 3f; // As long as speed is above this, keep sliding
    public float slideSteeringPower = 0.5f; // How much you can steer around while sliding

    [Header("Jump")] public float jumpForce;
    public float airMultiplier;
    private float lastSpeedBeforeTakeoff;

    [Header("Look Around")] public Transform cameraHolder;
    public float mouseSensitivity = 300f;
    public float verticalClampAngle = 90f;
    public float horizontalClampAngle = 90f;
    float verticalRotation = 0f;

    [Header("Ground Check")] public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")] public float maxSlopAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Transform References")] public GameObject playerCamera;
    public GameObject playerVisual;

    // Displacement Calculation
    Vector3 lastPosition;
    [HideInInspector] public Vector3 displacement;

    private Vector3 moveDirection;
    public Rigidbody rb;
    private PlayerUI playerUI;
    private InputHandler input;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerUI = GetComponent<PlayerUI>();
        input = GetComponent<InputHandler>();
    }

    private void Start()
    {
    }

    void Update()
    {
        HandleDrag();
        GroundCheck();
        playerUI.UpdateText("Velocity: " + rb.linearVelocity.magnitude.ToString("F2"));
        Debug.Log("Is Sliding: " + isSliding);
    }

    void FixedUpdate()
    {
        displacement =
            (transform.position - lastPosition) * 50;
        lastPosition = transform.position;
    }

    #region Movement

    public void ProcessMovement(Vector2 moveInput)
    {
        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (isSliding) return;

        if (moveDirection == Vector3.zero)
        {
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

    #endregion

    #region Look

    public void LookUpAndDownWithCamera(Vector2 lookInput)
    {
        float mouseY = lookInput.y * (Time.deltaTime * mouseSensitivity);

        // Update vertical rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

        // Apply vertical rotation to the camera holder
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    public void RotateBodyHorizontally(Vector2 lookInput)
    {
        // Get mouse input for horizontal rotation
        float mouseX = lookInput.x * (Time.deltaTime * mouseSensitivity);

        // Rotate the player horizontally
        transform.Rotate(0f, mouseX, 0f);

        // If sliding, allow steering slightly
        if (isSliding)
        {
            Vector3 newVelocity = rb.linearVelocity;
            newVelocity = Quaternion.Euler(0, mouseX * slideSteeringPower, 0) * newVelocity;
            rb.linearVelocity = newVelocity;
        }
    }

    #endregion

    #region Sliding

    public void Slide()
    {
        if (input.slideInput)
        {
            if (!isGrounded) return;
            if (isSliding) return;

            float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

            if (horizontalSpeed < slideSpeedThreshold) return;

            StartSliding();
        }


        if (isSliding)
        {
            Vector3 newVelocity = rb.linearVelocity * slideSpeedDampening;
            if (newVelocity.magnitude > keepSlidingSpeedThreshold) rb.linearVelocity = newVelocity;
            else StopSliding();
        }
    }

    private void StartSliding()
    {
        isSliding = true;
        cameraHolder.DOLocalMoveY(cameraHolder.localPosition.y - 0.4f, 0.2f);
        playerVisual.transform.DOLocalRotate(new Vector3(-20, 0, 0), 0.2f);
        playerVisual.transform.DOLocalMoveY(-0.2f, 0.2f);

        float currSpeedModifier = Mathf.Clamp(rb.linearVelocity.magnitude / 40, 0, 1); // Maximum speed at 20
        float boost = addedSlideSpeed + Mathf.Lerp(0, addedSlideSpeed * 2f, currSpeedModifier);

        Vector3 direction = rb.linearVelocity.normalized;
        rb.linearVelocity = direction * (displacement.magnitude + boost);
    }

    private void StopSliding()
    {
        Debug.Log("Stopping Sliding");
        isSliding = false;
        
        cameraHolder.DOLocalMoveY(cameraHolder.localPosition.y + 0.4f, 0.2f);
        playerVisual.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        playerVisual.transform.DOLocalMoveY(0f, 0.2f);
    }

    #endregion

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