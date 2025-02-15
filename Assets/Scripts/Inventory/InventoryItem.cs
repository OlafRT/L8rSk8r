using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;
    public GameObject itemPrefab; // The 3D model prefab used for the in-world item and preview
}

