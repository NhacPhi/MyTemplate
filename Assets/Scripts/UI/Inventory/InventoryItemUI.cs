using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemUI : GameItemUI, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanClick) return; // Chặn click nếu item chỉ để hiển thị (ví dụ trong màn hình nhận thưởng)
        
        UIEvent.OnSelectInventoryItem?.Invoke(id);
        OnSwitchStatusBoder(true);
    }

}
