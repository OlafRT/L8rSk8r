using System.Collections;
using UnityEngine;

public class BombDrop : MonoBehaviour
{
    [Header("Decal Settings")]
    [Tooltip("Prefab for the decal to indicate where the bomb will hit the ground.")]
    public GameObject decalPrefab;

    [Header("Kinematic Settings")]
    [Tooltip("Minimum delay (in seconds) before the bomb starts falling.")]
    public float kinematicDelayMin = 1f;
    [Tooltip("Maximum delay (in seconds) before the bomb starts falling.")]
    public float kinematicDelayMax = 2f;

    [Header("Explosion Settings")]
    [Tooltip("Prefab for the explosion effect.")]
    public GameObject explosionPrefab;
    [Tooltip("Damage to apply to the player.")]
    public int damageAmount = 5;
    [Tooltip("Radius of the explosion (and trigger area).")]
    public float explosionRadius = 5f;
    [Tooltip("How long (in seconds) the explosion trigger stays active.")]
    public float explosionTriggerDuration = 0.5f;

    [Header("Sound Settings")]
    [Tooltip("Sound to play when the bomb starts falling.")]
    public AudioClip fallingSound;
    [Tooltip("Sound to play when the bomb explodes.")]
    public AudioClip explosionSound;
    [Tooltip("AudioSource to play sounds. If not assigned, AudioSource.PlayClipAtPoint will be used.")]
    public AudioSource audioSource;

    [Header("Player Settings")]
    [Tooltip("Reference to the PlayerHealthController on the separate GameObject. " +
             "If unassigned, the script will attempt to find one in the scene.")]
    public PlayerHealthController playerHealthController;

    private Rigidbody rb;
    private SphereCollider explosionTrigger;
    private bool exploded = false;
    private GameObject decalInstance;  // Reference to the instantiated decal
    private bool decalSpawned = false;
    private bool damageApplied = false; // Ensure damage is only applied once

    void Start()
    {
        Debug.Log("BombDrop Start() called on " + gameObject.name);

        // Attempt to find the PlayerHealthController if not assigned.
        if (playerHealthController == null)
        {
            playerHealthController = FindObjectOfType<PlayerHealthController>();
            if (playerHealthController == null)
            {
                Debug.LogWarning("No PlayerHealthController found in the scene!");
            }
        }

        // Cast a ray downward to locate the ground and spawn the decal.
        if (!decalSpawned)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (decalPrefab != null)
                {
                    // Rotate decal so it's flat on the ground (90° about the X-axis).
                    Quaternion decalRotation = Quaternion.Euler(90f, 0f, 0f);
                    // Slight upward offset to prevent z-fighting.
                    Vector3 decalPosition = hit.point + Vector3.up * 0.01f;
                    decalInstance = Instantiate(decalPrefab, decalPosition, decalRotation);
                }
            }
            decalSpawned = true;
        }

        // Get the Rigidbody and set it to kinematic so it doesn't fall immediately.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to the bomb!");
        }
        else
        {
            rb.isKinematic = true;
        }

        // Get the AudioSource if not already assigned.
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Start the coroutine to disable kinematic after a random delay.
        float delay = Random.Range(kinematicDelayMin, kinematicDelayMax);
        StartCoroutine(EnablePhysicsAfterDelay(delay));

        // Prepare the explosion trigger collider.
        explosionTrigger = GetComponent<SphereCollider>();
        if (explosionTrigger == null)
        {
            explosionTrigger = gameObject.AddComponent<SphereCollider>();
        }
        explosionTrigger.isTrigger = true;
        explosionTrigger.radius = explosionRadius;
        explosionTrigger.enabled = false;
    }

    IEnumerator EnablePhysicsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            // Allow physics (gravity) to take effect.
            rb.isKinematic = false;

            // Play the falling sound.
            if (fallingSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(fallingSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(fallingSound, transform.position);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // When the bomb hits the ground, trigger the explosion.
        if (!exploded)
        {
            exploded = true;
            Explode();
        }
    }

    void Explode()
    {
        // Play the explosion sound.
        if (explosionSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
        }

        // Spawn the explosion effect.
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the decal once the bomb explodes.
        if (decalInstance != null)
        {
            Destroy(decalInstance);
        }

        // Enable the explosion trigger collider.
        explosionTrigger.enabled = true;

        // Apply damage once if the assigned player is within explosion range.
        if (!damageApplied && playerHealthController != null)
        {
            float dist = Vector3.Distance(transform.position, playerHealthController.transform.position);
            if (dist <= explosionRadius)
            {
                playerHealthController.TakeDamage(damageAmount);
                damageApplied = true;
            }
        }

        // After a brief period, disable the trigger and destroy the bomb.
        StartCoroutine(DisableTriggerAndDestroy());
    }

    IEnumerator DisableTriggerAndDestroy()
    {
        yield return new WaitForSeconds(explosionTriggerDuration);
        if (explosionTrigger != null)
        {
            explosionTrigger.enabled = false;
        }
        Destroy(gameObject);
    }

    // In case the player enters the explosion trigger after the explosion.
    void OnTriggerEnter(Collider other)
    {
        // Check for a collider tagged "Player" (adjust this tag if needed).
        if (exploded && other.CompareTag("Player") && playerHealthController != null && !damageApplied)
        {
            playerHealthController.TakeDamage(damageAmount);
            damageApplied = true;
        }
    }
}





