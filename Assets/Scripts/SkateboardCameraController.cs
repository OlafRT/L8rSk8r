using UnityEngine;

public class SkateboardCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform cameraRoot; // Assign SkateboardCameraRoot here
    private float xRotation = 0f;

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust rotation (clamp vertical rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the camera root
        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}