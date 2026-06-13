using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GachaItemCardUI : GameItemUI, IPointerClickHandler
{
    [SerializeField] private GameObject imgSelected;

    // Sự kiện trả về (BannerID, ItemID) khi click vào thẻ này
    public Action<string, string> OnCardClicked;
    
    private string _currentBannerId;
    private string _currentItemId;

    public void Setup(string bannerId, string itemId, Rare rare, Sprite icon, Sprite background)
    {
        _currentBannerId = bannerId;
        _currentItemId = itemId;
        
        // Gọi Setup của base class (GameItemUI) để tự động hiển thị Icon, Background và đổi màu Viền (Border) theo Rare
        base.Setup(itemId, rare, icon, background);
        ToggleSelected(false);
    }

    public void ToggleSelected(bool isSelected)
    {
        if (imgSelected != null)
        {
            imgSelected.SetActive(isSelected);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanClick) return;
        
        // Phát sự kiện chọn item trong Gacha thay vì sự kiện túi đồ
        OnCardClicked?.Invoke(_currentBannerId, _currentItemId);
    }
}
