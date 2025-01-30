using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponCollisionEffect : MonoBehaviour
{
    [Header("Visual/Particle")]
    [Tooltip("One or more particle effects to play when hit by a weapon.")]
    public ParticleSystem[] collisionEffects;

    [Header("Weapon/Attack Checking")]
    [Tooltip("Animator bool to check if the character is currently attacking.")]
    public string attackingBoolName = "IsAttacking";

    [Tooltip("The layer mask for the weapon layer.")]
    public LayerMask weaponLayer;

    [Header("Impact Settings")]
    [Tooltip("Number of hits required to destroy the object.")]
    public int hitsToDestroy = 1;

    // Internal state
    private bool isEffectPlaying = false;
    private int hitCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        // 1) Check if the collider is on the designated weapon layer
        if (!IsOnWeaponLayer(collision.gameObject)) return;

        // 2) Check if the character *really is* attacking right now
        if (!IsAttacking(collision.gameObject)) return;

        // 3) Conditions met => play effect(s) & increment hit count
        PlayEffectsAtPosition(collision.contacts[0].point);
        hitCount++;

        // 4) Destroy the object if we've reached required hits
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
    /// whose “attackingBoolName” is currently true.
    /// </summary>
    private bool IsAttacking(GameObject obj)
    {
        Animator anim = obj.transform.root.GetComponentInChildren<Animator>();
        if (anim == null) return false;
        return anim.GetBool(attackingBoolName);
    }

    /// <summary>
    /// Plays ALL assigned particle effects at the specified position (if not already playing).
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
                if (ps == null) continue;

                // Move the effect to the collision point & play
                ps.transform.position = position;
                ps.Play();

                // Track the longest effect duration
                float duration = ps.main.duration;
                if (duration > longestDuration) longestDuration = duration;
            }

            isEffectPlaying = true;

            // Reset the effect flag after the longest effect finishes
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
    /// Waits for the (longest) particle effect to finish, unparents them, then destroys the bag.
    /// </summary>
    private IEnumerator DestroyObjectAfterEffects()
    {
        // Find the longest effect duration
        float longestDuration = 0f;
        foreach (ParticleSystem ps in collisionEffects)
        {
            if (ps == null) continue;
            float duration = ps.main.duration;
            if (duration > longestDuration) longestDuration = duration;
        }

        // Wait for that duration
        yield return new WaitForSeconds(longestDuration);

        // 1) Un-parent each ParticleSystem so it remains after the trash bag is destroyed
        foreach (ParticleSystem ps in collisionEffects)
        {
            if (ps == null) continue;

            // Make it a root object in the scene (or setParent to some other transform)
            ps.transform.SetParent(null, true);

            // Optionally set 'Stop Action' in the inspector to 'Destroy' 
            // so they clean up themselves after finishing.
        }

        // 2) Finally destroy the main GameObject
        Destroy(gameObject);
    }
}




