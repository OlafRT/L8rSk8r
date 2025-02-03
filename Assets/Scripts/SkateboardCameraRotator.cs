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
    
    // Rotation limits for pitch (up and down movement)
    public float minPitch = -30f;
    public float maxPitch = 60f;

    private float pitch = 0f; // Current pitch angle (vertical rotation)

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

        // Apply rotation (clamping vertical rotation, disabling Z axis rotation)
        if (lookInput.sqrMagnitude > 0.1f)
        {
            // Horizontal (yaw) rotation around the Y axis (no clamp needed)
            float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
            target.Rotate(Vector3.up, yaw, Space.World);

            // Vertical (pitch) rotation around the X axis (clamped)
            pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);  // Clamp vertical rotation

            // Apply the clamped pitch to the target
            target.localRotation = Quaternion.Euler(pitch, target.localRotation.eulerAngles.y, 0); // Lock the Z-axis
        }
    }
}





