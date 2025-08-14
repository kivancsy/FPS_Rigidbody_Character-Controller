using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerCamera _playerCamera;
    PlayerMovement _playerMovement;
    private InputHandler _input;

    void Awake()
    {
        _input = GetComponent<InputHandler>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void FixedUpdate()
    {
        _playerMovement.ProcessMovement(_input.moveInput);
        
        if (_input.jumpInput)
            _playerMovement.Jump();
    }

    void LateUpdate()
    {
        _playerCamera.PlayerLook(_input.lookInput);
    }
}