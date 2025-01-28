using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollisionEffect : MonoBehaviour
{
    [Tooltip("The particle effect to play when a collision with the Weapon layer occurs.")]
    public ParticleSystem collisionEffect;

    [Tooltip("The minimum collision relative speed required to trigger the effect.")]
    public float minimumImpactSpeed = 5f;

    private int hitCount = 0; // Keeps track of the number of hits

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object is in the "Weapon" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            // Calculate the relative speed of the collision
            float impactSpeed = collision.relativeVelocity.magnitude;

            // Check if the impact speed meets or exceeds the threshold
            if (impactSpeed >= minimumImpactSpeed)
            {
                PlayEffect(collision.contacts[0].point); // Play the effect at the collision point

                hitCount++; // Increment hit count

                // Destroy the object after 2 hits
                if (hitCount >= 2)
                {
                    DestroyWithDelay();
                }
            }
        }
    }

    private void PlayEffect(Vector3 position)
    {
        if (collisionEffect != null)
        {
            // Move the particle effect to the collision position and play it
            collisionEffect.transform.position = position;
            collisionEffect.Play();
        }
        else
        {
            Debug.LogWarning("Collision effect not assigned in the inspector.");
        }
    }

    private void DestroyWithDelay()
    {
        // Wait for 0.5 seconds before destroying the object
        Invoke(nameof(DestroyObject), 0.5f);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}



