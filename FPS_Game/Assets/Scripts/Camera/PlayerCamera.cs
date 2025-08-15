using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }
}