using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;  // Needed for coroutines

public class ControllerSwitcher : MonoBehaviour
{
    [Header("Controllers and Cameras")]
    public GameObject thirdPersonController; // Third-person controller object
    public GameObject skateboardController;    // Skateboard controller object
    public GameObject skateboardPlayerFollowCamera; // Virtual camera for skateboard mode
    public GameObject playerFollowCamera;      // Virtual camera for third-person mode
    public GameObject skateboardCameraRoot;
    public GameObject playerCameraRoot;
    private CinemachineVirtualCamera camera;

    [Header("Mode-Specific Game Objects")]
    public GameObject[] skateboardModeEnableObjects;
    public GameObject[] skateboardModeDisableObjects;
    public GameObject[] thirdPersonModeEnableObjects;
    public GameObject[] thirdPersonModeDisableObjects;

    [Header("Input and Music")]
    public InputAction switchAction; // InputAction to toggle controllers
    public MusicManager musicManager; // Reference to the MusicManager script

    [Header("Animators and Animation Settings")]
    // Animators for switching animations (assign these in the Inspector)
    public Animator thirdPersonAnimator;  // Animator on the third-person controller
    public Animator skateboardAnimator;   // Animator on the skateboard controller

    // Trigger names used on the corresponding animators
    public string mountTrigger = "Mount";       // Trigger on thirdPersonAnimator when switching to skateboard mode
    public string dismountTrigger = "Dismount";   // Trigger on skateboardAnimator when switching to third-person mode

    // Delay durations to wait for the animations to finish before switching
    public float mountAnimationDuration = 1.0f;
    public float dismountAnimationDuration = 1.0f;

    [Header("Skateboard Visual Switch")]
    // The individual parts that make up the skateboard in skateboard mode.
    // These should be enabled when in skateboard mode.
    public GameObject[] skateboardComponents;
    // A single, combined skateboard model used only during dismounting.
    public GameObject combinedSkateboardModel;

    // Tracks the current mode (false = third-person, true = skateboard)
    private bool isSkateboardActive = false;

    void Start()
    {
        // Ensure third-person controller starts active
        thirdPersonController.SetActive(true);
        skateboardController.SetActive(false);

        // Make sure skateboard controller is a child of the third-person controller initially
        skateboardController.transform.SetParent(thirdPersonController.transform);

        // Check if the action is properly assigned
        if (switchAction == null)
        {
            Debug.LogError("SwitchAction is not assigned in the inspector!");
        }

        if (musicManager != null)
        {
            musicManager.SetPlaylist(musicManager.thirdPersonMusic); // Set default to third-person music
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
        if (switchAction != null && switchAction.triggered)
        {
            ToggleController();
        }
    }

    private void ToggleController()
    {
        if (!isSkateboardActive)
        {
            // Currently in third-person mode; switch to skateboard mode (mount).
            StartCoroutine(SwitchToSkateboardMode());
        }
        else
        {
            // Currently in skateboard mode; switch to third-person mode (dismount).
            StartCoroutine(SwitchToThirdPersonMode());
        }
    }

    private IEnumerator SwitchToSkateboardMode()
    {
        // When mounting, ensure that:
        //   - The combined board model is disabled (since it is only for dismounting), and
        //   - The individual skateboard parts are enabled (they are vital for proper skateboard visuals).
        if (combinedSkateboardModel != null)
        {
            combinedSkateboardModel.SetActive(false);
        }
        if (skateboardComponents != null)
        {
            foreach (GameObject part in skateboardComponents)
            {
                if (part != null) part.SetActive(true);
            }
        }

        // Play the mount animation on the third-person animator.
        if (thirdPersonAnimator != null)
        {
            thirdPersonAnimator.SetTrigger(mountTrigger);
        }

        // Wait for the mount animation to finish.
        yield return new WaitForSeconds(mountAnimationDuration);

        // Switch to skateboard mode:
        thirdPersonController.SetActive(false);  // Disable third-person controller.
        skateboardController.SetActive(true);      // Enable skateboard controller.

        // Adjust parent-child relationships:
        skateboardController.transform.SetParent(null);             // Unparent skateboard controller from third-person controller.
        thirdPersonController.transform.SetParent(skateboardController.transform);  // Parent third-person controller to skateboard controller.

        // Enable/disable game objects for skateboard mode:
        EnableDisableGameObjects(skateboardModeEnableObjects, true);
        EnableDisableGameObjects(skateboardModeDisableObjects, false);

        // Update the camera for skateboard mode:
        camera = skateboardPlayerFollowCamera.GetComponent<CinemachineVirtualCamera>();
        camera.Follow = skateboardCameraRoot.transform;

        if (musicManager != null)
        {
            musicManager.SetPlaylist(musicManager.skateboardMusic); // Switch to skateboard music.
        }

        // Update the mode flag.
        isSkateboardActive = true;

        // Reset the input action state to avoid carrying over inputs.
        if (switchAction != null)
        {
            switchAction.Disable();
            switchAction.Enable();
        }
    }

    private IEnumerator SwitchToThirdPersonMode()
    {
        // Before dismounting, switch the board visuals:
        // Hide the individual skateboard parts and enable the combined board model.
        if (combinedSkateboardModel != null)
        {
            if (skateboardComponents != null)
            {
                foreach (GameObject part in skateboardComponents)
                {
                    if (part != null) part.SetActive(false);
                }
            }
            combinedSkateboardModel.SetActive(true);
        }

        // Play the dismount animation on the skateboard animator.
        if (skateboardAnimator != null)
        {
            skateboardAnimator.SetTrigger(dismountTrigger);
        }

        // Wait for the dismount animation to finish.
        yield return new WaitForSeconds(dismountAnimationDuration);

        // Switch to third-person mode:
        thirdPersonController.SetActive(true);  // Enable third-person controller.
        skateboardController.SetActive(false);    // Disable skateboard controller.

        // Adjust parent-child relationships:
        thirdPersonController.transform.SetParent(null);           // Unparent third-person controller from skateboard controller.
        skateboardController.transform.SetParent(thirdPersonController.transform); // Parent skateboard controller to third-person controller.

        // Enable/disable game objects for third-person mode:
        EnableDisableGameObjects(thirdPersonModeEnableObjects, true);
        EnableDisableGameObjects(thirdPersonModeDisableObjects, false);

        // Update the camera for third-person mode:
        camera = playerFollowCamera.GetComponent<CinemachineVirtualCamera>();
        camera.Follow = playerCameraRoot.transform;

        if (musicManager != null)
        {
            musicManager.SetPlaylist(musicManager.thirdPersonMusic); // Switch to third-person music.
        }

        // After switching modes, disable the combined skateboard model.
        if (combinedSkateboardModel != null)
        {
            combinedSkateboardModel.SetActive(false);
        }

        // Update the mode flag.
        isSkateboardActive = false;

        // Reset the input action state.
        if (switchAction != null)
        {
            switchAction.Disable();
            switchAction.Enable();
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










