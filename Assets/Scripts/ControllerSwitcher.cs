using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class ControllerSwitcher : MonoBehaviour
{
    public GameObject thirdPersonController; // Third-person controller object
    public GameObject skateboardController; // Skateboard controller object

    public InputAction switchAction; // InputAction to toggle controllers

    private bool isSkateboardActive = false;
    public GameObject skateboardPlayerFollowCamera; // Game object with the virtual camera for skateboard mode
    public GameObject playerFollowCamera; // Game object with the virtual camera for third-person mode
    public GameObject skateboardCameraRoot;
    public GameObject playerCameraRoot;
    private CinemachineVirtualCamera camera;

    // Game objects to enable and disable when switching modes
    public GameObject[] skateboardModeEnableObjects;
    public GameObject[] skateboardModeDisableObjects;
    public GameObject[] thirdPersonModeEnableObjects;
    public GameObject[] thirdPersonModeDisableObjects;

    void Start()
    {
        // Ensure third-person controller starts active
        thirdPersonController.SetActive(true);
        skateboardController.SetActive(false);

        // Make sure skateboard controller is a child of the player initially
        skateboardController.transform.SetParent(thirdPersonController.transform);

        // Check if the action is properly assigned
        if (switchAction == null)
        {
            Debug.LogError("SwitchAction is not assigned in the inspector!");
        }
    }

    void OnEnable()
    {
        if (switchAction != null)
        {
            switchAction.Enable(); // Enable the input action when the object is enabled
        }
    }

    void OnDisable()
    {
        if (switchAction != null)
        {
            switchAction.Disable(); // Disable the input action when the object is disabled
        }
    }

    void Update()
    {
        // Check if the switch action is triggered
        if (switchAction != null && switchAction.triggered) // Use .triggered for input action events
        {
            ToggleController();
        }
    }

    private void ToggleController()
    {
        isSkateboardActive = !isSkateboardActive;

        if (isSkateboardActive)
        {
            // Switch to skateboard mode
            thirdPersonController.SetActive(false); // Disable third-person controller
            skateboardController.SetActive(true); // Enable skateboard controller

            // Unparent skateboard controller from third person controller
            skateboardController.transform.SetParent(null);

            // Make third-person controller a child of skateboard controller
            thirdPersonController.transform.SetParent(skateboardController.transform);

            // Enable and disable game objects for skateboard mode
            EnableDisableGameObjects(skateboardModeEnableObjects, true);
            EnableDisableGameObjects(skateboardModeDisableObjects, false);

            // Update the camera variable to point to the new camera
            camera = skateboardPlayerFollowCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();

            // Change the target of the Cinemachine Virtual Camera to the skateboard camera root
            camera.Follow = skateboardCameraRoot.transform;
        }
        else
        {
            // Switch to third-person mode
            thirdPersonController.SetActive(true); // Enable third-person controller
            skateboardController.SetActive(false); // Disable skateboard controller

            // Unparent third-person controller from skateboard controller
            thirdPersonController.transform.SetParent(null);

            // Make skateboard controller a child of third-person controller
            skateboardController.transform.SetParent(thirdPersonController.transform);

            // Enable and disable game objects for third-person mode
            EnableDisableGameObjects(thirdPersonModeEnableObjects, true);
            EnableDisableGameObjects(thirdPersonModeDisableObjects, false);

            // Update the camera variable to point to the regular camera
            camera = playerFollowCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();

            // Change the target of the Cinemachine Virtual Camera to the player camera root
            camera.Follow = playerCameraRoot.transform;
        }
    }

    private void EnableDisableGameObjects(GameObject[] objects, bool enable)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(enable);
        }
    }
}








