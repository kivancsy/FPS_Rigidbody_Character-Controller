using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerMovement _playerMovement;
    private InputHandler _input;

    void Awake()
    {
        _input = GetComponent<InputHandler>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        _playerMovement.ProcessMovement(_input.moveInput);

        if (_input.jumpInput)
            _playerMovement.Jump();

        _playerMovement.Slide();
    }

    void LateUpdate()
    {
        _playerMovement.LookUpAndDownWithCamera(_input.lookInput);
        _playerMovement.RotateBodyHorizontally(_input.lookInput);
    }
}