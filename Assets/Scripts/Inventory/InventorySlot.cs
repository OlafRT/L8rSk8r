using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    // Optionally store the item for further reference
    private InventoryItem currentItem;

    public void Setup(InventoryItem item)
    {
        currentItem = item;
        Debug.Log("InventorySlot: Setting up slot for " + item.itemName);

        // Find the ModelPreviewer (searching inactive children if necessary)
        ModelPreviewer previewer = GetComponentInChildren<ModelPreviewer>(true);
        if (previewer != null)
        {
            previewer.SetupPreview(item.itemPrefab);
        }
        else
        {
            Debug.LogError("InventorySlot: ModelPreviewer component not found!");
        }

        // Set the tooltip's item so that it knows what to display
        InventorySlotTooltip tooltip = GetComponent<InventorySlotTooltip>();
        if (tooltip != null)
        {
            tooltip.item = item;
        }
    }
}


