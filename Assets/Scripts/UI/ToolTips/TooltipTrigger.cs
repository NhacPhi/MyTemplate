using UnityEngine;
using UnityEngine.EventSystems;
using System;

public enum TooltipTriggerMode
{
    Battle,             // Dùng trong Battle: Click để chọn skill, Long press / Hover delay để xem tooltip
    CharacterScreen     // Dùng trong Character Screen: Hover hiện ngay lập tức, PointerExit ẩn ngay lập tức, Click/Tap để toggle
}

/// <summary>
/// Component đa nền tảng xử lý hover/long-press để hiển thị tooltip.
/// - Windows: Hover (PointerEnter/PointerExit)
/// - Android: Long Press / Tap Toggle
/// Gắn lên bất kỳ UI element nào cần tooltip.
/// </summary>
public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private TooltipTriggerMode _triggerMode = TooltipTriggerMode.Battle;
    [SerializeField] private float _longPressThreshold = Definition.TOOLTIP_LONG_PRESS_THRESHOLD;
    [SerializeField] private float _hoverDelay = Definition.TOOLTIP_HOVER_DELAY;

    public TooltipTriggerMode Mode => _triggerMode;

    /// <summary>
    /// Callback khi tooltip nên hiển thị.
    /// </summary>
    public event Action OnTooltipShow;

    /// <summary>
    /// Callback khi tooltip nên ẩn.
    /// </summary>
    public event Action OnTooltipHide;

    private float _pressTimer;
    private bool _isPressed;
    private bool _isHovering;
    private float _hoverTimer;
    private bool _isShowing;

    /// <summary>
    /// Thiết lập chế độ trigger cho Tooltip.
    /// </summary>
    public void SetTriggerMode(TooltipTriggerMode mode)
    {
        _triggerMode = mode;
    }

    /// <summary>
    /// Tương thích ngược: Bật/tắt chế độ CharacterScreen.
    /// </summary>
    public void SetTriggerOnClickOnMobile(bool enable)
    {
        _triggerMode = enable ? TooltipTriggerMode.CharacterScreen : TooltipTriggerMode.Battle;
    }

    private void Update()
    {
        if (_triggerMode == TooltipTriggerMode.Battle)
        {
            // 1. Battle Mobile Long Press
            if (_isPressed && !_isShowing)
            {
                _pressTimer += Time.unscaledDeltaTime;
                if (_pressTimer >= _longPressThreshold)
                {
                    ShowTooltip();
                }
            }

            // 2. Battle PC Hover Delay
            if (_isHovering && !_isShowing)
            {
                _hoverTimer += Time.unscaledDeltaTime;
                if (_hoverTimer >= _hoverDelay)
                {
                    ShowTooltip();
                }
            }
        }
    }

    // ═══════════════════════════════════════
    // Pointer Events
    // ═══════════════════════════════════════

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        _hoverTimer = 0f;

        if (_triggerMode == TooltipTriggerMode.CharacterScreen)
        {
            // Trong Character Scene: Di chuột vào là hiển thị ngay lập tức không cần chờ delay
            ShowTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        _hoverTimer = 0f;

        // Bất kể ở Battle hay Character Scene: Rê chuột ra ngoài icon là ẩn đi ngay lập tức
        HideTooltip();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        _pressTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        _pressTimer = 0f;

        // Nếu ở Battle và đang giữ hiện tooltip: Nhả tay ra thì ẩn
        if (_triggerMode == TooltipTriggerMode.Battle && _isShowing)
        {
            HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_triggerMode == TooltipTriggerMode.CharacterScreen)
        {
            // Trong Character Scene: Click/Tap có thể dùng để Toggle hiển thị trên thiết bị cảm ứng
            if (_isShowing)
            {
                HideTooltip();
            }
            else
            {
                ShowTooltip();
            }
        }
        else
        {
            // Trong Battle: Click là để chọn/dùng kỹ năng -> Hủy hover và ẩn tooltip ngay lập tức
            _isHovering = false;
            _hoverTimer = 0f;
            HideTooltip();
        }
    }

    // ═══════════════════════════════════════
    // Internal
    // ═══════════════════════════════════════

    private void ShowTooltip()
    {
        _isShowing = true;
        OnTooltipShow?.Invoke();
    }

    private void HideTooltip()
    {
        _isShowing = false;
        _isHovering = false;
        _hoverTimer = 0f;
        OnTooltipHide?.Invoke();
    }

    private void OnDisable()
    {
        HideTooltip();
        _isPressed = false;
        _isHovering = false;
        _pressTimer = 0f;
        _hoverTimer = 0f;
    }
}
