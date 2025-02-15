using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // The item for this slot. We assume the InventorySlot script sets this.
    public InventoryItem item;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
        {
            TooltipManager.Instance.ShowTooltip(item.itemName, item.description, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        TooltipManager.Instance.UpdateTooltipPosition(eventData.position);
    }
}

