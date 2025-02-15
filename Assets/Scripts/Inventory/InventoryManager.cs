using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // List to store picked-up items
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("UI Setup")]
    // Parent container (e.g., a Panel with a GridLayoutGroup) for inventory slots
    public Transform inventoryPanel;
    // The prefab for each inventory slot (set up as described previously)
    public GameObject inventorySlotPrefab;
    // The overall inventory UI panel (this should have a CanvasGroup attached)
    public GameObject inventoryUI;
    // The CanvasGroup on your inventoryUI
    public CanvasGroup inventoryCanvasGroup;

    private bool inventoryOpen = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        // Hide the UI via the CanvasGroup so its scripts remain active
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = 0;
            inventoryCanvasGroup.interactable = false;
            inventoryCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Call this method to add an item to the inventory.
    /// </summary>
    public void AddItem(InventoryItem item)
    {
        // Add the item to the list
        items.Add(item);

        // Instantiate a UI slot for the item
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryPanel);
        // Get the InventorySlot component on the prefab and set it up with this item
        InventorySlot slotScript = slot.GetComponent<InventorySlot>();
        if (slotScript != null)
            slotScript.Setup(item);
    }

    /// <summary>
    /// Set the visibility of the inventory UI.
    /// When open, enable the cursor.
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
        
        // Enable or disable the cursor
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




