using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Panel hiển thị chi tiết skill khi hover/long-press.
/// Nhận SkillTooltipData để update nội dung.
/// Tự động định vị theo vị trí con trỏ/ngón tay.
/// </summary>
public class SkillTooltipUI : MonoBehaviour, IPointerClickHandler
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

        EnsureCanvas();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        UIEvent.OnShowSkillTooltip -= Show;
        UIEvent.OnShowSkillTooltip += Show;
        UIEvent.OnShowSkillTooltipWithRect -= Show;
        UIEvent.OnShowSkillTooltipWithRect += Show;
        UIEvent.OnHideSkillTooltip -= Hide;
        UIEvent.OnHideSkillTooltip += Hide;
        UIEvent.OnHideAllToolTipUI -= Hide;
        UIEvent.OnHideAllToolTipUI += Hide;
        UIEvent.OnExecuteSkill -= Hide;
        UIEvent.OnExecuteSkill += Hide;
        UIEvent.OnChooseSkillCharacter -= OnChooseSkill;
        UIEvent.OnChooseSkillCharacter += OnChooseSkill;

        Hide();
    }

    private void OnChooseSkill(SkillCharacter skillType)
    {
        Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Hide();
    }

    private void OnDisable()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        UIEvent.OnShowSkillTooltip -= Show;
        UIEvent.OnShowSkillTooltipWithRect -= Show;
        UIEvent.OnHideSkillTooltip -= Hide;
        UIEvent.OnHideAllToolTipUI -= Hide;
        UIEvent.OnExecuteSkill -= Hide;
        UIEvent.OnChooseSkillCharacter -= OnChooseSkill;
    }

    private void EnsureCanvas()
    {
        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>(true);

        if (_parentCanvas == null)
            _parentCanvas = FindFirstObjectByType<Canvas>();
    }

    /// <summary>
    /// Hiển thị tooltip với data và RectTransform của element trigger.
    /// </summary>
    public void Show(SkillTooltipData data, RectTransform triggerRect)
    {
        Debug.Log($"[SkillTooltipUI] Show called for {data?.SkillName}");
        EnsureCanvas();
        UpdateContent(data);

        // Đưa tooltip lên trên cùng trong canvas để không bị các UI khác che khuất
        transform.SetAsLastSibling();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        UpdatePosition(triggerRect);
    }

    /// <summary>
    /// Hiển thị tooltip với data và screen position cụ thể.
    /// </summary>
    public void Show(SkillTooltipData data, Vector2 screenPosition)
    {
        Debug.Log($"[SkillTooltipUI] Show called for {data?.SkillName} at screenPos");
        EnsureCanvas();
        UpdateContent(data);

        transform.SetAsLastSibling();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        UpdatePosition(screenPosition);
    }

    /// <summary>
    /// Hiển thị tooltip mà không cần vị trí (dùng vị trí mặc định).
    /// </summary>
    public void Show(SkillTooltipData data)
    {
        EnsureCanvas();
        UpdateContent(data);
        transform.SetAsLastSibling();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
    }

    /// <summary>
    /// Ẩn tooltip.
    /// </summary>
    public void Hide()
    {
        Debug.Log("[SkillTooltipUI] Hide called");
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
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
    [SerializeField] private float _padding = 10f;

    /// <summary>
    /// Định vị tooltip chính xác dựa trên RectTransform của trigger element.
    /// </summary>
    public void UpdatePosition(RectTransform triggerRect)
    {
        if (triggerRect == null) return;
        EnsureCanvas();
        if (_tooltipRect == null) return;

        RectTransform parentRect = _tooltipRect.parent as RectTransform;
        if (parentRect == null && _parentCanvas != null)
            parentRect = _parentCanvas.transform as RectTransform;
        if (parentRect == null) return;

        // Force rebuild layout để lấy chính xác kích thước tooltip
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
        float tw = _tooltipRect.rect.width;
        float th = _tooltipRect.rect.height;

        // Lấy 4 góc của trigger trong World Space và chuyển sang Local Space của parentRect
        Vector3[] triggerCorners = new Vector3[4];
        triggerRect.GetWorldCorners(triggerCorners);

        Vector3 localBottomLeft = parentRect.InverseTransformPoint(triggerCorners[0]);
        Vector3 localTopLeft = parentRect.InverseTransformPoint(triggerCorners[1]);
        Vector3 localTopRight = parentRect.InverseTransformPoint(triggerCorners[2]);
        Vector3 localBottomRight = parentRect.InverseTransformPoint(triggerCorners[3]);
        Vector3 localCenter = parentRect.InverseTransformPoint(triggerRect.position);

        float triggerLeft = Mathf.Min(localBottomLeft.x, localTopLeft.x);
        float triggerRight = Mathf.Max(localBottomRight.x, localTopRight.x);
        float triggerBottom = Mathf.Min(localBottomLeft.y, localBottomRight.y);
        float triggerTop = Mathf.Max(localTopLeft.y, localTopRight.y);

        float parentCenterY = (parentRect.rect.yMin + parentRect.rect.yMax) * 0.5f;
        float parentCenterX = (parentRect.rect.xMin + parentRect.rect.xMax) * 0.5f;

        // ── Trục Y (Dọc) ──
        float pivotY;
        float posY;
        if (localCenter.y > parentCenterY)
        {
            // Trigger ở nửa trên màn hình (như Boss UI) → Tooltip mở xuống DƯỚI trigger
            pivotY = 1f;
            posY = triggerBottom - _padding;
        }
        else
        {
            // Trigger ở nửa dưới màn hình (như Player Skills) → Tooltip mở lên TRÊN trigger
            pivotY = 0f;
            posY = triggerTop + _padding;
        }

        // ── Trục X (Ngang) ──
        float pivotX;
        float posX;
        if (localCenter.x > parentCenterX)
        {
            // Trigger ở nửa phải màn hình → Căn mép phải tooltip theo mép phải trigger
            pivotX = 1f;
            posX = triggerRight;
        }
        else
        {
            // Trigger ở nửa trái màn hình → Căn mép trái tooltip theo mép trái trigger
            pivotX = 0f;
            posX = triggerLeft;
        }

        // ── Giới hạn trong vùng parentRect (Clamp an toàn) ──
        float margin = 15f;
        float parentMinX = parentRect.rect.xMin + margin;
        float parentMaxX = parentRect.rect.xMax - margin;
        float parentMinY = parentRect.rect.yMin + margin;
        float parentMaxY = parentRect.rect.yMax - margin;

        float minX = parentMinX + pivotX * tw;
        float maxX = parentMaxX - (1f - pivotX) * tw;
        if (minX <= maxX)
            posX = Mathf.Clamp(posX, minX, maxX);
        else
            posX = (parentMinX + parentMaxX) * 0.5f;

        float minY = parentMinY + pivotY * th;
        float maxY = parentMaxY - (1f - pivotY) * th;
        if (minY <= maxY)
            posY = Mathf.Clamp(posY, minY, maxY);
        else
            posY = (parentMinY + parentMaxY) * 0.5f;

        _tooltipRect.pivot = new Vector2(pivotX, pivotY);
        _tooltipRect.localPosition = new Vector3(posX, posY, 0f);
    }

    /// <summary>
    /// Fallback định vị qua Screen Position.
    /// </summary>
    private void UpdatePosition(Vector2 screenPosition)
    {
        EnsureCanvas();
        if (_parentCanvas == null || _tooltipRect == null) return;

        RectTransform parentRect = _tooltipRect.parent as RectTransform;
        if (parentRect == null)
            parentRect = _parentCanvas.transform as RectTransform;
        if (parentRect == null) return;

        Camera cam = (_parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _parentCanvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            cam,
            out Vector2 localPoint
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
        float tw = _tooltipRect.rect.width;
        float th = _tooltipRect.rect.height;

        float parentCenterY = (parentRect.rect.yMin + parentRect.rect.yMax) * 0.5f;
        float parentCenterX = (parentRect.rect.xMin + parentRect.rect.xMax) * 0.5f;

        float pivotY = (localPoint.y > parentCenterY) ? 1f : 0f;
        float posY = (pivotY == 1f) ? localPoint.y - _padding : localPoint.y + _padding;

        float pivotX = (localPoint.x > parentCenterX) ? 1f : 0f;
        float posX = localPoint.x;

        float margin = 15f;
        float parentMinX = parentRect.rect.xMin + margin;
        float parentMaxX = parentRect.rect.xMax - margin;
        float parentMinY = parentRect.rect.yMin + margin;
        float parentMaxY = parentRect.rect.yMax - margin;

        float minX = parentMinX + pivotX * tw;
        float maxX = parentMaxX - (1f - pivotX) * tw;
        if (minX <= maxX)
            posX = Mathf.Clamp(posX, minX, maxX);

        float minY = parentMinY + pivotY * th;
        float maxY = parentMaxY - (1f - pivotY) * th;
        if (minY <= maxY)
            posY = Mathf.Clamp(posY, minY, maxY);

        _tooltipRect.pivot = new Vector2(pivotX, pivotY);
        _tooltipRect.localPosition = new Vector3(posX, posY, 0f);
    }

}
