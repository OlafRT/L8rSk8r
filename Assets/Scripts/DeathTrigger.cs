using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    // Reference to the object that has the PlayerHealthController.
    // Assign this in the Inspector.
    [SerializeField] private PlayerHealthController playerHealthController;

    // The amount of damage to apply (should be high enough to kill the player).
    public int damageAmount = 9999;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player (make sure your player has the "Player" tag)
        if (other.CompareTag("Player"))
        {
            // If the reference has been assigned, call TakeDamage to kill the player.
            if (playerHealthController != null)
            {
                playerHealthController.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("PlayerHealthController is not assigned in KillPlayerOnTrigger.");
            }
        }
    }
}

