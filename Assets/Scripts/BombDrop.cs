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

    private Rigidbody rb;
    private SphereCollider explosionTrigger;
    private bool exploded = false;
    private GameObject decalInstance;  // Reference to the instantiated decal

    // Flag to ensure the decal is only spawned once.
    private bool decalSpawned = false;

    void Start()
    {
        // Debug log to check if Start is only being called once.
        Debug.Log("BombDrop Start() called on " + gameObject.name);

        // Cast a ray downward from the bomb's current position to find the ground.
        if (!decalSpawned)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (decalPrefab != null)
                {
                    // Set decal rotation to 90 degrees on the X-axis.
                    Quaternion decalRotation = Quaternion.Euler(90f, 0f, 0f);
                    // Add a slight upward offset to prevent z-fighting with the ground.
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

        // If no AudioSource was provided, try getting one from the GameObject.
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

        // Immediately check for any players within the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            PlayerHealthController playerHealth = col.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
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

    // In case a player enters the explosion radius after the explosion.
    void OnTriggerEnter(Collider other)
    {
        if (exploded)
        {
            PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}



