using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemUI : GameItemUI, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectInventoryItem?.Invoke(id);
        OnSwitchStatusBoder(true);
    }

}
