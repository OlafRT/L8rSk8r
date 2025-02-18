[System.Serializable]
public class InventoryEntry
{
    public InventoryItem item;
    public int quantity;
    public InventorySlot slot; // Reference to the UI slot for this entry.
}



