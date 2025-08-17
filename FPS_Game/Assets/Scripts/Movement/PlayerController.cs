using System;
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

    private void Update()
    {
    }

    void FixedUpdate()
    {
        _playerMovement.RotateBodyHorizontally(_input.lookInput);
        _playerMovement.RotateBodyHorizontally(_input.lookInput);

        _playerMovement.ProcessMovement(_input.moveInput);
        _playerMovement.Jump();
        _playerMovement.DashJump();

        _playerMovement.Slide();
        //_playerMovement.Dodge();
    }

    void LateUpdate()
    {
        _playerMovement.LookUpAndDownWithCamera(_input.lookInput);
    }
}