using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // Publicly store the current item for later reference.
    public InventoryItem currentItem;
    
    // Reference to the Text component to display the stack count.
    public Text stackCountText;

    /// <summary>
    /// Sets up the inventory slot with the given item and quantity.
    /// </summary>
    public void Setup(InventoryItem item, int quantity)
    {
        currentItem = item;
        Debug.Log("InventorySlot: Setting up slot for " + item.itemName);

        // Find the ModelPreviewer (searching inactive children if necessary).
        ModelPreviewer previewer = GetComponentInChildren<ModelPreviewer>(true);
        if (previewer != null)
        {
            // Pass the prefab and the preview scale factor from the item.
            previewer.SetupPreview(item.itemPrefab, item.previewScaleFactor);
        }
        else
        {
            Debug.LogError("InventorySlot: ModelPreviewer component not found!");
        }

        // Set the tooltip's item so that it knows what to display.
        InventorySlotTooltip tooltip = GetComponent<InventorySlotTooltip>();
        if (tooltip != null)
        {
            tooltip.item = item;
        }

        // If the item is stackable, display the quantity.
        if (item.stackable)
        {
            if (stackCountText != null)
            {
                stackCountText.gameObject.SetActive(true);
                stackCountText.text = quantity.ToString();
            }
        }
        else
        {
            if (stackCountText != null)
                stackCountText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the displayed stack count.
    /// </summary>
    public void UpdateStackCount(int quantity)
    {
        if (currentItem != null && currentItem.stackable && stackCountText != null)
        {
            stackCountText.text = quantity.ToString();
        }
    }
}






