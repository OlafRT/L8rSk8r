using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    // The panel that contains the tooltip UI.
    public GameObject tooltipPanel;
    // The Text component for the item name.
    public Text tooltipNameText;
    // The Text component for the item description.
    public Text tooltipDescriptionText;

    private RectTransform tooltipRectTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (tooltipPanel != null)
        {
            tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
            tooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the tooltip with the given text at the specified screen position.
    /// </summary>
    public void ShowTooltip(string itemName, string description, Vector2 position)
    {
        if (tooltipPanel != null)
        {
            tooltipNameText.text = itemName;
            tooltipDescriptionText.text = description;
            tooltipPanel.SetActive(true);
            UpdateTooltipPosition(position);
        }
    }

    /// <summary>
    /// Hides the tooltip (leaving the text as-is).
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Clears the tooltip text and hides the tooltip panel.
    /// Call this when, for example, the inventory is closed or an item is used.
    /// </summary>
    public void ClearTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        if (tooltipNameText != null)
        {
            tooltipNameText.text = "";
        }
        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text = "";
        }
    }

    /// <summary>
    /// Updates the tooltip panel's position.
    /// </summary>
    public void UpdateTooltipPosition(Vector2 position)
    {
        if (tooltipRectTransform != null)
        {
            tooltipRectTransform.position = position;
        }
    }
}


