using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerInputSet input;

    public Vector2 lookInput;
    public Vector2 moveInput;

    void Awake()
    {
        input = new PlayerInputSet();
        SubscribeToInputActionEvents();
    }

    private void SubscribeToInputActionEvents()
    {
        input.PlayerController.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.PlayerController.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();
}