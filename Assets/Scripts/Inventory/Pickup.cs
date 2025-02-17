using UnityEngine;

public class Pickup : MonoBehaviour
{
    // Reference to the InventoryItem ScriptableObject for this pickup.
    public InventoryItem item;

    // Sound to play when this item is picked up.
    public AudioClip pickupSound;

    // Optional volume control for the pickup sound.
    [Range(0f, 1f)]
    public float pickupSoundVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add the item to the inventory.
            InventoryManager.Instance.AddItem(item);
            
            // Play the pickup sound if one is assigned.
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }
            
            // Destroy this pickup from the scene.
            Destroy(gameObject);
        }
    }
}


