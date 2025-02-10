using UnityEngine;
using System.Collections;

public class ImpactReceiver : MonoBehaviour
{
    [Tooltip("The layer mask for the weapon layer.")]
    public LayerMask weaponLayer;

    [Header("Impact Settings")]
    [Tooltip("Number of hits required to destroy the object.")]
    public int hitsToDestroy = 1;

    [Header("Impact Sound")]
    [Tooltip("AudioSource used to play impact sounds. If not assigned, the AudioSource on this GameObject is used.")]
    public AudioSource impactAudioSource;
    [Tooltip("Impact sounds to choose from when hit.")]
    public AudioClip[] impactClips;

    [Header("Impact Sound Variation")]
    [Tooltip("Minimum pitch multiplier (e.g., 0.95).")]
    public float minPitch = 0.95f;
    [Tooltip("Maximum pitch multiplier (e.g., 1.05).")]
    public float maxPitch = 1.05f;

    [Header("Attack & Effects Settings")]
    [Tooltip("Name of the bool in the Animator that indicates an attack is occurring.")]
    public string attackingBoolName;
    [Tooltip("Particle effects to play when hit.")]
    public ParticleSystem[] collisionEffects;

    // Internal state
    private bool isEffectPlaying = false;
    private int hitCount = 0;

    private void Start()
    {
        // If no AudioSource is assigned, get the one on this GameObject.
        if (impactAudioSource == null)
        {
            impactAudioSource = GetComponent<AudioSource>();
            if (impactAudioSource == null)
            {
                Debug.LogWarning("No AudioSource component found on " + gameObject.name + ". Impact sound will not play.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1) Check if the collider is on the designated weapon layer.
        if (!IsOnWeaponLayer(collision.gameObject))
            return;

        // 2) Check if the character *really is* attacking right now.
        if (!IsAttacking(collision.gameObject))
            return;

        // 3) Conditions met => play impact sound with pitch variation, effects & increment hit count.
        if (impactAudioSource != null && impactClips != null && impactClips.Length > 0)
        {
            // Select a random impact clip from the array.
            AudioClip selectedClip = impactClips[Random.Range(0, impactClips.Length)];

            // Store the original pitch so it can be restored later.
            float originalPitch = impactAudioSource.pitch;
            // Set a random pitch within the desired range.
            impactAudioSource.pitch = Random.Range(minPitch, maxPitch);
            impactAudioSource.PlayOneShot(selectedClip);
            // Restore the original pitch.
            impactAudioSource.pitch = originalPitch;
        }

        // Play particle effects at the collision contact point.
        PlayEffectsAtPosition(collision.contacts[0].point);

        // Increment hit count.
        hitCount++;

        // 4) Destroy the object if we've reached the required number of hits.
        if (hitCount >= hitsToDestroy)
        {
            StartCoroutine(DestroyObjectAfterEffects());
        }
    }

    /// <summary>
    /// Checks if the collision’s GameObject is on the “weapon” layer.
    /// </summary>
    private bool IsOnWeaponLayer(GameObject obj)
    {
        return (weaponLayer.value & (1 << obj.layer)) != 0;
    }

    /// <summary>
    /// Returns true if the collision’s root or parent has an Animator 
    /// whose "attackingBoolName" is currently true.
    /// </summary>
    private bool IsAttacking(GameObject obj)
    {
        Animator anim = obj.transform.root.GetComponentInChildren<Animator>();
        if (anim == null)
            return false;
        return anim.GetBool(attackingBoolName);
    }

    /// <summary>
    /// Plays all assigned particle effects at the specified position (if not already playing).
    /// </summary>
    private void PlayEffectsAtPosition(Vector3 position)
    {
        if (collisionEffects == null || collisionEffects.Length == 0)
            return;

        if (!isEffectPlaying)
        {
            float longestDuration = 0f;

            foreach (ParticleSystem ps in collisionEffects)
            {
                if (ps == null)
                    continue;

                // Move the effect to the collision point & play.
                ps.transform.position = position;
                ps.Play();

                // Track the longest effect duration.
                float duration = ps.main.duration;
                if (duration > longestDuration)
                    longestDuration = duration;
            }

            isEffectPlaying = true;

            // Reset the effect flag after the longest effect finishes.
            if (longestDuration > 0f)
                Invoke(nameof(ResetEffectFlag), longestDuration);
            else
                ResetEffectFlag();
        }
    }

    private void ResetEffectFlag()
    {
        isEffectPlaying = false;
    }

    /// <summary>
    /// Waits for the (longest) particle effect to finish, unparents them, then destroys the object.
    /// </summary>
    private IEnumerator DestroyObjectAfterEffects()
    {
        // Find the longest effect duration.
        float longestDuration = 0f;
        foreach (ParticleSystem ps in collisionEffects)
        {
            if (ps == null)
                continue;
            float duration = ps.main.duration;
            if (duration > longestDuration)
                longestDuration = duration;
        }

        // Wait for that duration.
        yield return new WaitForSeconds(longestDuration);

        // Unparent each ParticleSystem so it remains after the object is destroyed.
        foreach (ParticleSystem ps in collisionEffects)
        {
            if (ps == null)
                continue;
            ps.transform.SetParent(null, true);
        }

        // Finally, destroy the main GameObject.
        Destroy(gameObject);
    }
}

