using UnityEngine;
using UnityEngine.InputSystem;

public class SkateboardCameraRotator : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float rotationSpeed = 5f;
    public float smoothTime = 0.2f;

    private Vector2 lookInput;
    private Vector3 currentVelocity;

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked; // Ensure the cursor is hidden and locked
    }

    void Update()
    {
        if (target == null) return;

        // Update the camera position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // Get input for rotation
        lookInput = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue() != Vector2.zero)
            lookInput = Gamepad.current.rightStick.ReadValue();

        // Apply rotation
        if (lookInput.sqrMagnitude > 0.1f)
        {
            float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
            float pitch = lookInput.y * rotationSpeed * Time.deltaTime;
            target.Rotate(Vector3.up, yaw, Space.World);
            target.Rotate(Vector3.right, -pitch, Space.Self);
        }
    }
}