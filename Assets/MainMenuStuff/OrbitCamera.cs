using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target the camera will orbit around.")]
    public Transform target;

    [Header("Orbit Settings")]
    [Tooltip("Distance from the target.")]
    public float distance = 5f;
    [Tooltip("Speed of horizontal rotation (degrees per unit input).")]
    public float horizontalSpeed = 100f;
    [Tooltip("Speed of vertical rotation (degrees per unit input).")]
    public float verticalSpeed = 100f;
    [Tooltip("Minimum vertical angle in degrees.")]
    public float minVerticalAngle = -20f;
    [Tooltip("Maximum vertical angle in degrees.")]
    public float maxVerticalAngle = 80f;

    [Header("Input Settings")]
    public bool invertX = false;
    public bool invertY = false;
#if ENABLE_INPUT_SYSTEM
    [Tooltip("Input Action for look input (Vector2).")]
    public InputActionReference lookInput;
#endif

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitCamera: No target assigned.");
            enabled = false;
            return;
        }

        // Initialize yaw and pitch based on the camera's starting position relative to the target.
        Vector3 offset = transform.position - target.position;
        distance = offset.magnitude;
        yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;

        UpdateCameraPosition();
    }

    private void Update()
    {
        Vector2 lookDelta = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        if (lookInput != null)
        {
            lookDelta = lookInput.action.ReadValue<Vector2>();
        }
#else
        lookDelta.x = Input.GetAxis("Mouse X");
        lookDelta.y = Input.GetAxis("Mouse Y");
#endif

        // Apply inversion based on the boolean flags.
        if (invertX)
            lookDelta.x = -lookDelta.x;
        if (invertY)
            lookDelta.y = -lookDelta.y;

        // Update yaw and pitch based on input and speed.
        yaw += lookDelta.x * horizontalSpeed * Time.deltaTime;
        // Subtract lookDelta.y as usual (if not inverted, it already works correctly)
        pitch -= lookDelta.y * verticalSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Compute the new rotation.
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        // Calculate the offset from the target.
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}

