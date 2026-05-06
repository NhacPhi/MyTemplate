using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Component đa nền tảng xử lý hover/long-press để hiển thị tooltip.
/// - Windows: Hover (PointerEnter/PointerExit)
/// - Android: Long Press (PointerDown giữ > threshold)
/// Gắn lên bất kỳ UI element nào cần tooltip.
/// </summary>
public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float _longPressThreshold = Definition.TOOLTIP_LONG_PRESS_THRESHOLD;
    [SerializeField] private float _hoverDelay = Definition.TOOLTIP_HOVER_DELAY;

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
    private bool _isTooltipShowing;
    private bool _isHovering;
    private float _hoverTimer;

    private void Update()
    {
        // Android: Long press detection
        if (_isPressed && !_isTooltipShowing)
        {
            _pressTimer += Time.unscaledDeltaTime;
            if (_pressTimer >= _longPressThreshold)
            {
                ShowTooltip();
            }
        }

        // Windows: Hover delay
        if (_isHovering && !_isTooltipShowing && !IsTouchDevice())
        {
            _hoverTimer += Time.unscaledDeltaTime;
            if (_hoverTimer >= _hoverDelay)
            {
                ShowTooltip();
            }
        }
    }

    // ═══════════════════════════════════════
    // Pointer Events
    // ═══════════════════════════════════════

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsTouchDevice()) return;

        _isHovering = true;
        _hoverTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsTouchDevice())
        {
            // Trên mobile, exit không ẩn tooltip (dùng tap ngoài để ẩn)
            return;
        }

        _isHovering = false;
        _hoverTimer = 0f;
        HideTooltip();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsTouchDevice()) return;

        _isPressed = true;
        _pressTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsTouchDevice()) return;

        _isPressed = false;
        _pressTimer = 0f;

        // Nếu tooltip đang hiện, nhả tay sẽ ẩn
        if (_isTooltipShowing)
        {
            HideTooltip();
        }
    }

    // ═══════════════════════════════════════
    // Internal
    // ═══════════════════════════════════════

    private void ShowTooltip()
    {
        if (_isTooltipShowing) return;
        _isTooltipShowing = true;
        OnTooltipShow?.Invoke();
    }

    private void HideTooltip()
    {
        if (!_isTooltipShowing) return;
        _isTooltipShowing = false;
        OnTooltipHide?.Invoke();
    }

    private bool IsTouchDevice()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    private void OnDisable()
    {
        // Cleanup khi disable
        if (_isTooltipShowing)
        {
            HideTooltip();
        }
        _isPressed = false;
        _isHovering = false;
        _pressTimer = 0f;
        _hoverTimer = 0f;
    }
}
