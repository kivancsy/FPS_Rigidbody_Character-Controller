using System.Collections;
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

    [Header("Sliding")] public float slideSpeedThreshold = 6f;

    public float
        addedSlideSpeed = 3f;

    public float
        slideSpeedDampening = 0.99f;

    public float keepSlidingSpeedThreshold = 3f;
    public float slideSteeringPower = 0.5f;

    [Header("Dash Settings")] [SerializeField]
    private float dashJumpForce = 20f;

    public bool isDashing;
    private bool canDash = true;
    private Vector3 dashDirection;
    private float dashTimer;

    [SerializeField] private float dashUpwardForce = 0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Jump")] public float jumpForce;
    private float lastSpeedBeforeTakeoff;
    private bool shouldSlideOnLand = false;

    [Header("Look Around")] public Transform cameraHolder;
    public float mouseSensitivity = 300f;
    public float verticalClampAngle = 90f;
    float verticalRotation = 0f;

    [Header("Ground Check")] public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")] public float maxSlopAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Transform References")] public GameObject playerCamera;
    public GameObject playerVisual;
    [SerializeField] private Transform weaponHolder;

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
        GroundCheck();
        playerUI.UpdateText("Velocity: " + rb.linearVelocity.magnitude.ToString("F2"));
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
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        float horizontalSpeed = new Vector3(displacement.x, 0, displacement.y).magnitude;
        float speedToApply = Mathf.Max(baseSpeed, horizontalSpeed);

        if (!isGrounded) speedToApply = lastSpeedBeforeTakeoff;
        if (speedToApply > baseSpeed) speedToApply *= isGrounded ? 0.985f : 0.99f;


        Vector3 newVelocity = moveDirection.normalized * speedToApply;
        newVelocity.y = rb.linearVelocity.y;

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
        if (input.jumpInput)
        {
            exitingSlope = true;
            if (!isGrounded) return;
            if (isSliding) return;

           // lastSpeedBeforeTakeoff = displacement.magnitude;

            rb.linearVelocity += Vector3.up * jumpForce;
        }


        // rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void DashJump()
    {
        if (input.dashJumpInput)
        {
            shouldSlideOnLand = true;
            exitingSlope = true;
            if (isSliding) return;
            if (!isGrounded) return;
            lastSpeedBeforeTakeoff = displacement.magnitude;

            Vector2 inputDirection = input.moveInput;
            Vector3 jumpDir = (transform.forward * inputDirection.y + transform.right * inputDirection.x).normalized;

            if (jumpDir == Vector3.zero) return;
            else
                jumpDir = (jumpDir + Vector3.up).normalized;

            rb.linearVelocity += jumpDir * dashJumpForce;

            StartCoroutine(SlideOnLandCoroutine());
        }
    }

    private IEnumerator SlideOnLandCoroutine()
    {
        while (!isGrounded)
            yield return null;

        Slide();
    }

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
        weaponHolder.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    public void RotateBodyHorizontally(Vector2 lookInput)
    {
        float mouseX = lookInput.x * (Time.fixedDeltaTime * mouseSensitivity);

        //transform.Rotate(0f, mouseX, 0f);
        Quaternion deltaRotation = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);

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
        if (input.slideInput || shouldSlideOnLand)
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

    void RotateWeapon()
    {
        weaponHolder.DOLocalMoveY(weaponHolder.localPosition.y - 0.55f, 0.2f);
        weaponHolder.DOLocalMoveX(weaponHolder.localPosition.x - 0.39f, 0.2f);
        weaponHolder.DOLocalMoveZ(weaponHolder.localPosition.z + 0.3f, 0.2f);
        weaponHolder.DOLocalRotate(new Vector3(0, 5, 0), 0.2f);
    }

    void ReturnWeaponBackToRoot()
    {
        weaponHolder.DOLocalMoveY(weaponHolder.localPosition.y + 0.55f, 0.2f);
        weaponHolder.DOLocalMoveX(weaponHolder.localPosition.x + 0.39f, 0.2f);
        weaponHolder.DOLocalMoveZ(weaponHolder.localPosition.z - 0.3f, 0.2f);
        weaponHolder.DOLocalRotate(new Vector3(0, 5, 0), 0.2f);
    }

    private void StartSliding()
    {
        isSliding = true;
        cameraHolder.DOLocalMoveY(cameraHolder.localPosition.y - 0.4f, 0.2f);
        playerVisual.transform.DOLocalRotate(new Vector3(-20, 0, 0), 0.2f);
        playerVisual.transform.DOLocalMoveY(-0.2f, 0.2f);
        RotateWeapon();

        float currSpeedModifier = Mathf.Clamp(rb.linearVelocity.magnitude / 40, 0, 1);
        float boost = addedSlideSpeed + Mathf.Lerp(0, addedSlideSpeed * 2f, currSpeedModifier);

        Vector3 direction = rb.linearVelocity.normalized;
        rb.linearVelocity = direction * (displacement.magnitude + boost);
    }

    private void StopSliding()
    {
        isSliding = false;
        cameraHolder.DOLocalMoveY(cameraHolder.localPosition.y + 0.4f, 0.2f);
        playerVisual.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        playerVisual.transform.DOLocalMoveY(0f, 0.2f);

        ReturnWeaponBackToRoot();
    }

    #endregion

    #region Dodge

    // public void Dodge()
    // {
    //     if (input.dashJumpInput && canDash && isGrounded && !isSliding && !isDashing)
    //     {
    //         StartDodge();
    //     }
    // }
    //
    // private void StartDodge()
    // {
    //     isDashing = true;
    //     canDash = false;
    //
    //     // Input yönünü al
    //     Vector2 inputDir = input.moveInput;
    //     Vector3 moveDir = Vector3.zero;
    //
    //     if (inputDir.x < 0) moveDir = Vector3.left; // A
    //     else if (inputDir.x > 0) moveDir = Vector3.right; // D
    //     else if (inputDir.y < 0) moveDir = Vector3.back; // S
    //     else
    //     {
    //         // ileri (W) veya input yoksa dodge iptal
    //         isDashing = false;
    //         canDash = true;
    //         return;
    //     }
    //
    //     // Force uygula (Dash’ten kopya)
    //     Vector3 force = moveDir.normalized * dashForce;
    //     rb.linearVelocity = Vector3.zero; // önce mevcut velocity’yi sıfırla, daha temiz olur
    //     rb.AddForce(force, ForceMode.Impulse);
    //
    //     // Süre sonunda bitir
    //     Invoke(nameof(ResetDash), dashDuration);
    // }
    //
    // private void StopDodge()
    // {
    //     isDashing = false;
    // }
    //
    //
    // private void ResetDash()
    // {
    //     canDash = true;
    // }

    #endregion

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        if (isGrounded)
        {
            exitingSlope = false;
            shouldSlideOnLand = false;
        }
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