using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerInputSet input;
    private PlayerMovement playerMovement;
    public Vector2 lookInput { get; private set; }
    public Vector2 moveInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool slideInput { get; private set; }

    void Awake()
    {
        input = new PlayerInputSet();
        playerMovement = GetComponent<PlayerMovement>();
        SubscribeToInputActionEvents();
    }

    void Update()
    {
        Debug.Log("Slide Input: " + slideInput);
    }

    private void SubscribeToInputActionEvents()
    {
        input.PlayerController.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.PlayerController.Look.canceled += ctx => lookInput = Vector2.zero;

        input.PlayerController.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.PlayerController.Movement.canceled += ctx => moveInput = Vector2.zero;

        input.PlayerController.Jump.performed += ctx => jumpInput = ctx.ReadValueAsButton();
        input.PlayerController.Jump.canceled += ctx => jumpInput = false;

        input.PlayerController.Slide.performed += ctx => slideInput = true;
        input.PlayerController.Slide.canceled += ctx => slideInput = false;
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();
}