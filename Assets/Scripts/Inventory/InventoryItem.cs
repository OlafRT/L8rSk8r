using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;
    public GameObject itemPrefab; // The 3D model prefab used for the in-world item and preview

    // New fields for stacking and preview scaling.
    public bool stackable = false;    // Set true for coins/money, etc.
    public int maxStack = 99;         // Maximum number that can stack
    public float previewScaleFactor = 1f; // Scale factor for inventory preview (1 = original size)
}



