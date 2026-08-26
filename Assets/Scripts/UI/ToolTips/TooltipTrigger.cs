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
    IPointerUpHandler,
    IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private float _longPressThreshold = Definition.TOOLTIP_LONG_PRESS_THRESHOLD;
    [SerializeField] private float _hoverDelay = Definition.TOOLTIP_HOVER_DELAY;
    [Tooltip("Nếu true: Chỉ cần click/tap để hiện tooltip (dùng cho CharacterScene). Nếu false: Cần giữ long-press (dùng cho BattleUIScene).")]
    [SerializeField] private bool _triggerOnClickOnMobile = false;

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
    /// Bật/tắt chế độ click/tap để hiện tooltip (dùng cho CharacterScene).
    /// </summary>
    public void SetTriggerOnClickOnMobile(bool enable)
    {
        _triggerOnClickOnMobile = enable;
    }

    private void Update()
    {
        // 1. Mobile Long Press (Áp dụng khi KHÔNG ở chế độ click-on-mobile, tức là trong Battle)
        if (_isPressed && !_isShowing && !_triggerOnClickOnMobile)
        {
            _pressTimer += Time.unscaledDeltaTime;
            if (_pressTimer >= _longPressThreshold)
            {
                ShowTooltip();
            }
        }

        // 2. PC / Windows Hover (Chỉ hiển thị sau khi hover đủ 0.5s)
        if (_isHovering && !_isShowing)
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
        _isHovering = true;
        _hoverTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        _hoverTimer = 0f;

        // Nếu không phải đang mở tooltip bằng click trong CharacterScene -> di chuột ra ngoài sẽ ẩn
        if (!_triggerOnClickOnMobile)
        {
            HideTooltip();
        }
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

        // Nếu ở chế độ Long Press (trong Battle) và tooltip đang hiện: Nhấc tay sẽ ẩn
        if (!_triggerOnClickOnMobile && _isShowing)
        {
            HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_triggerOnClickOnMobile)
        {
            // Trong CharacterScene: Click / Tap để bật/tắt (Toggle) tooltip
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
