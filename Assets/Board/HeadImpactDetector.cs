using UnityEngine;

// Attach this script to an empty child GameObject placed at the player's head.
// Ensure it has a collider (set as non-trigger) so that it collides with the ground.
public class HeadImpactDetector : MonoBehaviour
{
    [Tooltip("Reference to the PlayerHealthController.")]
    public PlayerHealthController playerHealthController;
    [Tooltip("Minimum impact speed to register a head hit.")]
    public float impactThreshold = 5f;
    [Tooltip("Multiplier to calculate damage from impact speed.")]
    public float damageMultiplier = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        // Optionally check collision against specific layers (e.g., ground).
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed > impactThreshold)
        {
            int damage = Mathf.RoundToInt((impactSpeed - impactThreshold) * damageMultiplier);
            if (damage > 0 && playerHealthController != null)
            {
                playerHealthController.TakeDamage(damage);
            }
        }
    }
}

