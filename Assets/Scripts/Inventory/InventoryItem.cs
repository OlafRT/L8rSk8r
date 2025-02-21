using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;
    public GameObject itemPrefab; // The 3D model prefab for in-world item and preview

    // Fields for stacking and preview scaling.
    public bool stackable = false;
    public int maxStack = 99;
    public float previewScaleFactor = 1f;

    // NEW: Heal amount (if > 0, this item heals the player when used)
    public int healAmount = 0;
}




