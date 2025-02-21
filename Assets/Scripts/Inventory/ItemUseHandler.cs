using UnityEngine;

public class ItemUseHandler : MonoBehaviour
{
    [Tooltip("Reference to the InventorySlot that holds this item.")]
    public InventorySlot inventorySlot;

    [Tooltip("Reference to the PlayerHealthController in the scene (will be found automatically if not assigned).")]
    public PlayerHealthController playerHealth;

    [Tooltip("Optional: Reference to the player's Animator to trigger an eating animation.")]
    public Animator playerAnimator;

    [Tooltip("Name of the trigger to play when the item is used (e.g., 'Eat').")]
    public string useAnimationTrigger = "Eat";

    [Header("Sound Settings")]
    [Tooltip("Sound to play when the item is used.")]
    public AudioClip useSound;
    [Tooltip("Volume for the use sound (0 to 1).")]
    [Range(0f, 1f)]
    public float useSoundVolume = 1f;

    private void Start()
    {
        // Auto-find the PlayerHealthController if not assigned.
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealthController>();
            if (playerHealth == null)
            {
                Debug.LogWarning("PlayerHealthController not found in the scene.");
            }
        }
        // Optionally, auto-find the player's Animator (if it's on the same GameObject or a child).
        if (playerAnimator == null && playerHealth != null)
        {
            playerAnimator = playerHealth.GetComponentInChildren<Animator>();
        }
    }

    // This method should be linked to the UI button's OnClick event.
    public void UseItem()
    {
        if (inventorySlot == null || inventorySlot.currentItem == null)
        {
            Debug.LogWarning("No item in this slot to use.");
            return;
        }

        InventoryItem item = inventorySlot.currentItem;

        // Check if the item is usable (in this case, if healAmount > 0).
        if (item.healAmount > 0)
        {
            // Prevent usage if the player is already at full health.
            if (playerHealth != null && playerHealth.IsAtFullHealth())
            {
                Debug.Log("Player is already at full health. Cannot use " + item.itemName);
                return;
            }

            // Heal the player.
            if (playerHealth != null)
            {
                playerHealth.Heal(item.healAmount);
                Debug.Log("Player healed by " + item.healAmount);
            }
            else
            {
                Debug.LogWarning("PlayerHealthController not found.");
            }

            // Optionally, play an eating animation.
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(useAnimationTrigger);
            }

            // Play the use sound, if assigned.
            if (useSound != null)
            {
                // Find the main player by tag.
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector3 soundPosition = (player != null) ? player.transform.position : transform.position;
                {
                    // Fallback to PlayClipAtPoint using the player's position.
                    AudioSource.PlayClipAtPoint(useSound, soundPosition, useSoundVolume);
                }
            }

            // Remove one instance of the item from the inventory.
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(item);
            }

            // Hide the tooltip using your TooltipManager.
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltip();
            }
        }
        else
        {
            Debug.Log("This item is not usable.");
        }
    }
}




