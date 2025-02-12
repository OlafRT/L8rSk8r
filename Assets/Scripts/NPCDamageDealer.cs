using UnityEngine;

public class NPCDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of health to deduct from the player on hit (default is 1).")]
    public int damageAmount = 1;
    [Tooltip("Should this NPC be able to deal damage? Enable this only during the proper attack window via Animation Events.")]
    public bool canDealDamage = false;

    [Header("Player Hit Reaction")]
    [Tooltip("Reference to the player's Animator (assign this in the Inspector).")]
    public Animator playerAnimator;
    [Tooltip("Reference to the PlayerHealthController (assign this in the Inspector).")]
    public PlayerHealthController playerHealthController;
    [Tooltip("Name of the trigger to invoke on the player's Animator for a soft hit (damage == 1).")]
    public string softHitTrigger = "SoftHit";
    [Tooltip("Name of the trigger to invoke on the player's Animator for a hard hit (damage == 3).")]
    public string hardHitTrigger = "HardHit";

    /// <summary>
    /// Called via an Animation Event to enable damage.
    /// </summary>
    public void EnableDamage()
    {
        canDealDamage = true;
    }

    /// <summary>
    /// Called via an Animation Event to disable damage.
    /// </summary>
    public void DisableDamage()
    {
        canDealDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage)
            return;

        if (other.CompareTag("Player"))
        {
            // Do not apply damage if the player is already dead.
            if (PlayerHealthController.isPlayerDead)
                return;

            if (playerHealthController != null)
            {
                playerHealthController.TakeDamage(damageAmount);
            }

            if (playerAnimator != null)
            {
                // Choose a reaction based on the damage amount.
                if (damageAmount == 1)
                {
                    playerAnimator.SetTrigger(softHitTrigger);
                }
                else if (damageAmount == 3)
                {
                    playerAnimator.SetTrigger(hardHitTrigger);
                }
                // Optionally: add additional cases for other damage values.
            }
            
            // Optionally, you can disable damage after the first hit in the window:
            canDealDamage = false;
        }
    }
}



