using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using StarterAssets;  // Assumes your SkateboardController and StarterAssetsInputs are in this namespace

public class SkateboardExtras : MonoBehaviour
{
    [Header("Air Grab Settings")]
    [Tooltip("Enable air grabbing functionality.")]
    public bool enableAirGrab = true;
    [Tooltip("Multiplier to increase air rotation sensitivity when grabbing.")]
    public float grabRotationMultiplier = 3f;
    private bool isGrabbing = false;

    [Header("Grind Settings")]
    [Tooltip("Enable grinding functionality.")]
    public bool enableGrinding = true;
    [Tooltip("Speed along the rail during grinding.")]
    public float grindSpeed = 10f;
    private bool isGrinding = false;
    [Tooltip("Target rail endpoint to grind toward (set by GrindRail).")]
    public Transform currentRailEnd;

    [Header("Grind Visual Limits")]
    [Tooltip("Maximum allowed tilt (roll) angle in degrees during grinding.")]
    public float maxGrindTiltAngle = 15f;

    [Header("Animator Settings")]
    [Tooltip("Animator used to play extra animations (e.g., 'isGrabbing' and 'isGrinding').")]
    public Animator extrasAnimator;

    // Cache a reference to the existing SkateboardController.
    private SkateboardController skateboardController;
#if ENABLE_INPUT_SYSTEM
    private StarterAssetsInputs input;
#endif

    // Store default rotation sensitivity so we can restore it.
    private float defaultRotationSensitivityX;
    private float defaultRotationSensitivityY;

    void Awake()
    {
        skateboardController = GetComponent<SkateboardController>();
#if ENABLE_INPUT_SYSTEM
        input = GetComponent<StarterAssetsInputs>();
#endif
        // Save the default sensitivities from the SkateboardController.
        defaultRotationSensitivityX = skateboardController.rotationSensitivityX;
        defaultRotationSensitivityY = skateboardController.rotationSensitivityY;
    }

    void Update()
    {
        // --- Air Grab Functionality ---
        // Always reset grab state if we become grounded.
        if (skateboardController.isGrounded && isGrabbing)
        {
            isGrabbing = false;
            // Reset sensitivities to defaults.
            skateboardController.rotationSensitivityX = defaultRotationSensitivityX;
            skateboardController.rotationSensitivityY = defaultRotationSensitivityY;
            if (extrasAnimator != null)
            {
                extrasAnimator.SetBool("isGrabbing", false);
            }
        }
        else if (enableAirGrab && !skateboardController.isGrounded)
        {
#if ENABLE_INPUT_SYSTEM
            bool grabPressed = input.grab;
#else
            bool grabPressed = Input.GetButton("Grab");
#endif
            if (grabPressed && !isGrabbing)
            {
                isGrabbing = true;
                // Increase rotation sensitivity.
                skateboardController.rotationSensitivityX = defaultRotationSensitivityX * grabRotationMultiplier;
                skateboardController.rotationSensitivityY = defaultRotationSensitivityY * grabRotationMultiplier;
                if (extrasAnimator != null)
                {
                    extrasAnimator.SetBool("isGrabbing", true);
                }
            }
            else if (!grabPressed && isGrabbing)
            {
                isGrabbing = false;
                // Reset rotation sensitivity.
                skateboardController.rotationSensitivityX = defaultRotationSensitivityX;
                skateboardController.rotationSensitivityY = defaultRotationSensitivityY;
                if (extrasAnimator != null)
                {
                    extrasAnimator.SetBool("isGrabbing", false);
                }
            }
        }

        // --- Grind Functionality ---
        if (enableGrinding && isGrinding && currentRailEnd != null)
        {
            Vector3 direction = (currentRailEnd.position - transform.position).normalized;
            transform.position += direction * grindSpeed * Time.deltaTime;
            // Clamp the board's Z rotation (roll) while grinding.
            ClampGrindTilt();

            // If near the endpoint, stop grinding.
            if (Vector3.Distance(transform.position, currentRailEnd.position) < 1f)
            {
                StopGrind();
            }
        }
    }

    /// <summary>
    /// Clamps the board's Z-axis rotation (roll) to prevent excessive tilting.
    /// </summary>
    private void ClampGrindTilt()
    {
        // Get current Euler angles.
        Vector3 euler = transform.eulerAngles;
        // Convert z from 0-360 to a signed angle (-180,180).
        float zAngle = euler.z;
        if (zAngle > 180f) 
            zAngle -= 360f;
        // Clamp the z angle.
        zAngle = Mathf.Clamp(zAngle, -maxGrindTiltAngle, maxGrindTiltAngle);
        // Convert back to 0-360 if needed.
        euler.z = (zAngle < 0) ? 360f + zAngle : zAngle;
        transform.eulerAngles = euler;
    }

    /// <summary>
    /// Called by a GrindRail script to start grinding.
    /// </summary>
    public void StartGrind(Transform railEnd)
    {
        if (!enableGrinding)
            return;
        isGrinding = true;
        currentRailEnd = railEnd;
        if (extrasAnimator != null)
        {
            extrasAnimator.SetBool("isGrinding", true);
        }
        // Optionally disable normal controls.
        if (skateboardController != null)
        {
            skateboardController.enabled = false;
        }
    }

    /// <summary>
    /// Stops grinding and re-enables normal controls.
    /// </summary>
    public void StopGrind()
    {
        isGrinding = false;
        currentRailEnd = null;
        if (extrasAnimator != null)
        {
            extrasAnimator.SetBool("isGrinding", false);
        }
        if (skateboardController != null)
        {
            skateboardController.enabled = true;
        }
    }
}





