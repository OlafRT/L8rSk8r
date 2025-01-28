using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerSwitcher : MonoBehaviour
{
    public GameObject thirdPersonController; // Third-person controller object
    public GameObject skateboardController; // Skateboard controller object

    public InputAction switchAction; // InputAction to toggle controllers

    private bool isSkateboardActive = false;

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
        }
    }
}








