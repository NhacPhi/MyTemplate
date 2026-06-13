using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GachaTargetSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image imgBackground;
    [SerializeField] private Image imgIcon;
    [SerializeField] private GameObject emptyOverlay; // Hình ảnh overlay hiển thị khi chưa chọn

    // Sự kiện trả về (BannerID, ItemID) khi click vào slot này
    public Action<string, string> OnSlotClicked;
    
    private string _currentBannerId;
    private string _currentItemId;
    private bool _canClick = true;

    /// <summary>
    /// Hiển thị trạng thái chưa chọn mục tiêu (Empty)
    /// </summary>
    public void SetupEmpty(string bannerId)
    {
        _currentBannerId = bannerId;
        _currentItemId = string.Empty;
        
        if (emptyOverlay != null) emptyOverlay.SetActive(true);
        if (imgIcon != null) imgIcon.gameObject.SetActive(false);
        
        _canClick = true;
    }

    /// <summary>
    /// Hiển thị trạng thái đã chọn mục tiêu (Filled)
    /// </summary>
    public void SetupFilled(string bannerId, string itemId, Rare rare, Sprite icon, Sprite background)
    {
        _currentBannerId = bannerId;
        _currentItemId = itemId;
        
        if (emptyOverlay != null) emptyOverlay.SetActive(false);
        
        if (imgIcon != null)
        {
            imgIcon.gameObject.SetActive(true);
            imgIcon.sprite = icon;
        }

        if (imgBackground != null)
        {
            imgBackground.sprite = background;
        }
        
        _canClick = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_canClick) return;
        
        OnSlotClicked?.Invoke(_currentBannerId, _currentItemId);
    }
}
