using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // List to store picked-up items as InventoryEntries.
    public List<InventoryEntry> items = new List<InventoryEntry>();

    [Header("UI Setup")]
    // Parent container (e.g., a Panel with a GridLayoutGroup) for inventory slots.
    public Transform inventoryPanel;
    // The prefab for each inventory slot (set up as described below).
    public GameObject inventorySlotPrefab;
    // The overall inventory UI panel (this should have a CanvasGroup attached).
    public GameObject inventoryUI;
    // The CanvasGroup on your inventoryUI.
    public CanvasGroup inventoryCanvasGroup;

    private bool inventoryOpen = false;

    private void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        // Hide the UI via the CanvasGroup so its scripts remain active.
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = 0;
            inventoryCanvasGroup.interactable = false;
            inventoryCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Call this method to add an item to the inventory.
    /// For stackable items, increases quantity if an entry already exists.
    /// </summary>
    public void AddItem(InventoryItem item)
    {
        if (item.stackable)
        {
            // Try to find an existing entry for this item.
            InventoryEntry existingEntry = items.Find(e => e.item.itemName == item.itemName);
            if (existingEntry != null)
            {
                // Increase quantity if not exceeding maxStack.
                if (existingEntry.quantity < item.maxStack)
                {
                    existingEntry.quantity++;
                    if (existingEntry.slot != null)
                        existingEntry.slot.UpdateStackCount(existingEntry.quantity);
                }
                else
                {
                    Debug.Log("Max stack reached for " + item.itemName);
                }
                return;
            }
        }

        // Otherwise, create a new inventory entry.
        InventoryEntry newEntry = new InventoryEntry();
        newEntry.item = item;
        newEntry.quantity = 1;

        // Instantiate a UI slot for the item.
        GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryPanel);
        InventorySlot slotScript = slotObj.GetComponent<InventorySlot>();
        if (slotScript != null)
        {
            slotScript.Setup(item, newEntry.quantity);
            newEntry.slot = slotScript;
        }
        items.Add(newEntry);
    }

    /// <summary>
    /// Removes one instance of the specified item from the inventory.
    /// For stackable items, decrements the quantity; if quantity reaches zero, removes the slot.
    /// For non-stackable items, removes the slot.
    /// </summary>
    public void RemoveItem(InventoryItem item)
    {
        InventoryEntry entry = items.Find(e => e.item.itemName == item.itemName);
        if (entry != null)
        {
            if (entry.item.stackable)
            {
                entry.quantity--;
                if (entry.quantity <= 0)
                {
                    if (entry.slot != null)
                        Destroy(entry.slot.gameObject);
                    items.Remove(entry);
                }
                else
                {
                    if (entry.slot != null)
                        entry.slot.UpdateStackCount(entry.quantity);
                }
            }
            else
            {
                if (entry.slot != null)
                    Destroy(entry.slot.gameObject);
                items.Remove(entry);
            }
        }
    }

    /// <summary>
    /// Returns true if the inventory contains the specified item.
    /// </summary>
    public bool HasItem(InventoryItem item)
    {
        return items.Exists(e => e.item.itemName == item.itemName);
    }

    /// <summary>
    /// Sets the visibility of the inventory UI.
    /// When open, enables the cursor.
    /// </summary>
    public void SetInventoryOpen(bool open)
    {
        inventoryOpen = open;
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = inventoryOpen ? 1 : 0;
            inventoryCanvasGroup.interactable = inventoryOpen;
            inventoryCanvasGroup.blocksRaycasts = inventoryOpen;
        }
        
        // Enable or disable the cursor.
        if (inventoryOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}






