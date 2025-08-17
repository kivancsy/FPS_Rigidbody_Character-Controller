using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerInputSet input;
    private PlayerMovement playerMovement;
    public Vector2 lookInput { get; private set; }
    public Vector2 moveInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool slideInput { get; private set; }
    public bool dashJumpInput { get; private set; }
    public bool shootInput;

    void Awake()
    {
        input = new PlayerInputSet();
        playerMovement = GetComponent<PlayerMovement>();
        SubscribeToInputActionEvents();
    }

    private void SubscribeToInputActionEvents()
    {
        input.PlayerController.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.PlayerController.Look.canceled += ctx => lookInput = Vector2.zero;

        input.PlayerController.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.PlayerController.Movement.canceled += ctx => moveInput = Vector2.zero;

        input.PlayerController.Jump.performed += ctx => jumpInput = ctx.ReadValueAsButton();
        input.PlayerController.Jump.canceled += ctx => jumpInput = false;

        input.PlayerController.Slide.started += ctx => slideInput = true;
        input.PlayerController.Slide.canceled += ctx => slideInput = false;

        input.PlayerController.Dash.performed += ctx => dashJumpInput = true;
        input.PlayerController.Dash.canceled += ctx => dashJumpInput = false;

        input.PlayerController.Shoot.performed += ctx => shootInput = true;
        input.PlayerController.Shoot.canceled += ctx => shootInput = false;
        
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();
}