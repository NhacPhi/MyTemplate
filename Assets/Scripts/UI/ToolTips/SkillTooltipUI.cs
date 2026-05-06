using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel hiển thị chi tiết skill khi hover/long-press.
/// Nhận SkillTooltipData để update nội dung.
/// Tự động định vị theo vị trí con trỏ/ngón tay.
/// </summary>
public class SkillTooltipUI : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Image _iconSkill;
    [SerializeField] private TextMeshProUGUI _txtSkillName;
    [SerializeField] private TextMeshProUGUI _txtSkillDescription;
    [SerializeField] private TextMeshProUGUI _txtCooldown;

    [Header("Layout")]
    [SerializeField] private RectTransform _tooltipRect;
    [SerializeField] private Canvas _parentCanvas;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (_tooltipRect == null)
            _tooltipRect = GetComponent<RectTransform>();

        // includeInactive = true để tìm Canvas kể cả khi parent bị inactive
        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>(true);

        // Fallback: tìm root Canvas trong scene
        if (_parentCanvas == null)
            _parentCanvas = FindAnyObjectByType<Canvas>();

        // CanvasGroup blocksRaycasts = false → tooltip không chặn pointer events
        // Tránh bug: chuột đè lên tooltip → trigger nhận PointerExit → flickering
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        UIEvent.OnShowSkillTooltip += Show;
        UIEvent.OnHideSkillTooltip += Hide;
        UIEvent.OnHideAllToolTipUI += Hide;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UIEvent.OnShowSkillTooltip -= Show;
        UIEvent.OnHideSkillTooltip -= Hide;
        UIEvent.OnHideAllToolTipUI -= Hide;
    }

    /// <summary>
    /// Hiển thị tooltip với data và vị trí cụ thể.
    /// </summary>
    public void Show(SkillTooltipData data, Vector2 screenPosition)
    {
        UpdateContent(data);

        // Phải active TRƯỚC khi tính vị trí,
        // vì RectTransform trả về kích thước 0 khi inactive.
        gameObject.SetActive(true);

        UpdatePosition(screenPosition);
    }

    /// <summary>
    /// Hiển thị tooltip mà không cần vị trí (dùng vị trí mặc định).
    /// </summary>
    public void Show(SkillTooltipData data)
    {
        UpdateContent(data);
        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
    }

    /// <summary>
    /// Ẩn tooltip.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ═══════════════════════════════════════
    // Private
    // ═══════════════════════════════════════

    private void UpdateContent(SkillTooltipData data)
    {
        if (_iconSkill != null && data.Icon != null)
        {
            _iconSkill.sprite = data.Icon;
        }

        if (_txtSkillName != null)
        {
            _txtSkillName.text = data.SkillName;
        }

        if (_txtSkillDescription != null)
        {
            _txtSkillDescription.text = data.SkillDescription;
        }

        if (_txtCooldown != null)
        {
            _txtCooldown.text = data.MaxCooldown.ToString();
        }
    }

    [Header("Offset")]
    [Tooltip("Khoảng cách giữa tooltip và trigger element")]
    [SerializeField] private float _padding = 8f;


    private void UpdatePosition(Vector2 screenPosition)
    {
        if (_parentCanvas == null || _tooltipRect == null) return;

        var canvasRect = _parentCanvas.transform as RectTransform;
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float halfW = canvasWidth * 0.5f;
        float halfH = canvasHeight * 0.5f;

        // Lấy đúng camera theo render mode
        Camera cam = (_parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _parentCanvas.worldCamera
            : null;

        // Chuyển screen position của trigger sang local position trong canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            cam,
            out Vector2 anchorPoint
        );

        // Force rebuild để lấy kích thước tooltip chính xác
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
        float tw = _tooltipRect.rect.width;
        float th = _tooltipRect.rect.height;

        // ── Mặc định: hiện bên PHẢI trigger ──
        // Pivot (0, 0) = góc dưới-trái → tooltip mở rộng sang phải và lên trên
        _tooltipRect.pivot = new Vector2(0f, 0f);
        float posX = anchorPoint.x + _padding;

        // Nếu tràn phải → flip sang bên TRÁI
        if (posX + tw > halfW)
        {
            _tooltipRect.pivot = new Vector2(1f, 0f); // góc dưới-phải
            posX = anchorPoint.x - _padding;
        }

        // ── Dọc: đáy tooltip ngang hàng với trigger ──
        float posY = anchorPoint.y;

        // Clamp để không tràn canvas
        posX = Mathf.Clamp(posX, -halfW, halfW);
        posY = Mathf.Clamp(posY, -halfH, halfH - th);

        _tooltipRect.anchoredPosition = new Vector2(posX, posY);
    }

}
