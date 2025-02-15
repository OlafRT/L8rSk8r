using UnityEngine;

public class Pickup : MonoBehaviour
{
    // Reference to the InventoryItem ScriptableObject for this pickup
    public InventoryItem item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add the item to the inventory
            InventoryManager.Instance.AddItem(item);
            // Optionally, destroy this pickup from the scene
            Destroy(gameObject);
        }
    }
}

