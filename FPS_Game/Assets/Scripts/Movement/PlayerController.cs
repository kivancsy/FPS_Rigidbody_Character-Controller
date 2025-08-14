using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerCamera _playerCamera;

    private InputHandler _input;

    void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    void FixedUpdate()
    {
    }

    void LateUpdate()
    {
        _playerCamera.PlayerLook(_input.lookInput);
    }
}