using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Make the transform look at the camera.
        // The position is offset by the camera's forward vector.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, 
                         mainCamera.transform.rotation * Vector3.up);
    }
}

