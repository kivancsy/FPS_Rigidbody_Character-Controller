using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;
    private PlayerMovement playerMovement;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void PlayerLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * (Time.deltaTime * sensitivityX);
        float mouseY = lookInput.y * (Time.deltaTime * sensitivityY);

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        if (playerMovement.isSliding)
        {
            Vector3 newVelocity = playerMovement.rb.linearVelocity;
            newVelocity = Quaternion.Euler(0, mouseX * playerMovement.slideSteeringPower, 0) * newVelocity;
            playerMovement.rb.linearVelocity = newVelocity;
        }
    }
}