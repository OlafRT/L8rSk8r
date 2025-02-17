using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Player Transform. If left empty, the player will be assigned on trigger enter.")]
    public Transform player;

    [Header("Rotation Settings")]
    [Tooltip("How quickly the NPC body rotates to face the player.")]
    public float rotationSpeed = 5f;
    
    [Header("Neck Joint Settings")]
    [Tooltip("The neck joint Transform (from your NPC’s rig) that should tilt toward the player.")]
    public Transform neckJoint;
    [Tooltip("How quickly the neck joint rotates.")]
    public float neckRotationSpeed = 5f;
    [Tooltip("Minimum pitch (in degrees) for the neck joint.")]
    public float minPitch = -30f;
    [Tooltip("Maximum pitch (in degrees) for the neck joint.")]
    public float maxPitch = 30f;

    // Store the initial local rotation of the neck joint so we can blend relative to it.
    private Quaternion initialNeckRotation;
    
    // Track whether the player is in the trigger zone.
    private bool playerInTrigger = false;

    private void Start()
    {
        // Store the initial rotation of the neck joint if one was assigned.
        if (neckJoint != null)
        {
            initialNeckRotation = neckJoint.localRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // When the player enters, set our flag and assign the player Transform if needed.
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            if (player == null)
            {
                player = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When the player leaves, clear the flag.
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    private void Update()
    {
        // Only run our look/neck logic when the player is in the trigger zone.
        if (playerInTrigger && player != null)
        {
            // --- 1. Rotate NPC Body Towards the Player ---
            Vector3 direction = player.position - transform.position;
            direction.y = 0f; // keep only horizontal direction
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // --- 2. Tilt the Neck Joint Toward the Player ---
            if (neckJoint != null)
            {
                // Calculate the vector from the neck joint to the player.
                Vector3 neckToPlayer = player.position - neckJoint.position;
                
                // Separate horizontal and vertical distances.
                Vector3 horizontal = neckToPlayer;
                horizontal.y = 0f;
                float horizontalDistance = horizontal.magnitude;
                float verticalDistance = neckToPlayer.y;
                
                // Calculate the desired pitch (in degrees) based on the vertical/horizontal distances.
                // (For example, if the player is lower than the neck, this will be negative.)
                float desiredPitch = Mathf.Atan2(verticalDistance, horizontalDistance) * Mathf.Rad2Deg;
                desiredPitch = Mathf.Clamp(desiredPitch, minPitch, maxPitch);
                
                // Compute a target rotation for the neck joint that adjusts only its x (pitch) relative to its original rotation.
                Quaternion targetNeckRotation = Quaternion.Euler(desiredPitch, 0f, 0f) * initialNeckRotation;
                
                // Smoothly interpolate the neck joint's local rotation toward the target.
                neckJoint.localRotation = Quaternion.Slerp(neckJoint.localRotation, targetNeckRotation, neckRotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Optionally, when the player is not in trigger, return the neck joint to its initial rotation.
            if (neckJoint != null)
            {
                neckJoint.localRotation = Quaternion.Slerp(neckJoint.localRotation, initialNeckRotation, neckRotationSpeed * Time.deltaTime);
            }
        }
    }
}

