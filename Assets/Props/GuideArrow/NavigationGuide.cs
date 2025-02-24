using UnityEngine;

public class NavigationGuide : MonoBehaviour
{
    // Static singleton for external access.
    public static NavigationGuide CurrentGuide { get; private set; }
    // Carries over the last target between mode switches.
    public static Transform CarriedTarget { get; private set; }

    [Header("Arrow References")]
    // The arrow for mode 1 (active at start).
    public Transform arrowMode1;
    // The arrow for mode 2 (disabled at start).
    public Transform arrowMode2;
    // The currently active arrow.
    private Transform currentArrow;

    [Header("References")]
    // Set to the Main Camera so it remains constant.
    public Transform playerTransform;
    // The navigation target in world space.
    public Transform currentTarget;

    [Header("Settings")]
    // How quickly the arrow rotates.
    public float rotationSpeed = 5f;

    [Header("Offsets")]
    // Additional rotational offset in degrees to fine-tune the arrow's alignment.
    public float rotationalOffset = 0f;

    // Mode flag: true for mode 1, false for mode 2.
    private bool isMode1 = true;

    private void Awake()
    {
        CurrentGuide = this;
        // Ensure playerTransform is assigned; if not, assign the Main Camera.
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
        // Initialize currentArrow to the arrow for mode 1.
        currentArrow = arrowMode1;
        if (arrowMode1 != null) arrowMode1.gameObject.SetActive(true);
        if (arrowMode2 != null) arrowMode2.gameObject.SetActive(false);
        
        // If a target was carried over, reassign it.
        if (CarriedTarget != null)
        {
            currentTarget = CarriedTarget;
            if (currentArrow != null)
                currentArrow.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // Check for the mode switch key ("E").
        if (Input.GetKeyDown(KeyCode.E))
        {
            isMode1 = !isMode1;
            SwitchArrow();
        }
    }

    private void LateUpdate()
    {
        // Only update if we have a target and a valid arrow.
        if (currentTarget == null || currentArrow == null || playerTransform == null)
            return;

        // Calculate the world-space direction from the arrow to the target.
        Vector3 worldDirection = currentTarget.position - currentArrow.position;
        // Optionally ignore vertical differences (for horizontal rotation only).
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude > 0.001f)
        {
            // Compute the desired world rotation so the arrow's forward (Z+) points toward the target.
            Quaternion desiredWorldRotation = Quaternion.LookRotation(worldDirection);
            // Convert world rotation into a local rotation relative to the player.
            Quaternion desiredLocalRotation = Quaternion.Inverse(playerTransform.rotation) * desiredWorldRotation;
            // Apply the additional offset (rotationalOffset in degrees) to fine-tune the arrow's orientation.
            Quaternion offsetRotation = Quaternion.Euler(0, rotationalOffset, 0);
            Quaternion finalRotation = desiredLocalRotation * offsetRotation;
            // Smoothly interpolate the arrow's local rotation.
            currentArrow.localRotation = Quaternion.Slerp(currentArrow.localRotation, finalRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // Switches the active arrow when modes are toggled.
    private void SwitchArrow()
    {
        if (isMode1)
        {
            currentArrow = arrowMode1;
            if (arrowMode1 != null) arrowMode1.gameObject.SetActive(true);
            if (arrowMode2 != null) arrowMode2.gameObject.SetActive(false);
        }
        else
        {
            currentArrow = arrowMode2;
            if (arrowMode2 != null) arrowMode2.gameObject.SetActive(true);
            if (arrowMode1 != null) arrowMode1.gameObject.SetActive(false);
        }
    }

    // Sets the navigation target and stores it for mode switches.
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        CarriedTarget = newTarget;
        if (currentArrow != null)
            currentArrow.gameObject.SetActive(true);
    }

    // Clears the target and hides the arrow.
    public void ClearTarget()
    {
        currentTarget = null;
        CarriedTarget = null;
        if (currentArrow != null)
            currentArrow.gameObject.SetActive(false);
    }
}











