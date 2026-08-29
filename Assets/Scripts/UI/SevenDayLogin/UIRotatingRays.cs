using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Component tạo hiệu ứng Tia sáng tỏa tròn xoay đều (Rotating Sunburst Rays)
/// Hoàn toàn bằng code procedural, không cần texture hay asset ngoài.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIRotatingRays : MonoBehaviour
{
    [Header("Ray Visual Settings")]
    [SerializeField] private Color rayColor = new Color(1f, 0.88f, 0.35f, 0.45f); // Vàng kim tỏa sáng
    [SerializeField] private int rayCount = 12; // Số tia sáng tỏa ra
    [SerializeField] private float raySharpness = 2.0f; // Độ sắc nét của chùm tia

    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 25f; // Độ xoay mỗi giây (dương = cùng chiều, âm = ngược chiều)
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseScaleAmount = 1.06f;
    [SerializeField] private float pulseDuration = 1.2f;

    private RectTransform _rectTransform;
    private Image _rayImage;
    private GameObject _rayGo;
    private bool _isBuilt = false;
    private Tween _pulseTween;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        BuildProceduralRays();
    }

    private void OnEnable()
    {
        if (!_isBuilt) BuildProceduralRays();
        StartRaysAnimation();
    }

    private void OnDisable()
    {
        StopRaysAnimation();
    }

    private void OnDestroy()
    {
        StopRaysAnimation();
    }

    private void Update()
    {
        if (_rayGo != null && _rayGo.activeSelf)
        {
            _rayGo.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }

    public void SetColor(Color color)
    {
        rayColor = color;
        if (_rayImage != null) _rayImage.color = rayColor;
    }

    public void Play()
    {
        if (!_isBuilt) BuildProceduralRays();
        if (_rayGo != null) _rayGo.SetActive(true);
        StartRaysAnimation();
    }

    public void Stop()
    {
        StopRaysAnimation();
        if (_rayGo != null) _rayGo.SetActive(false);
    }

    public void SetVisible(bool isVisible)
    {
        if (isVisible) Play();
        else Stop();
    }

    private void BuildProceduralRays()
    {
        if (_isBuilt && _rayGo != null) return;

        var existing = transform.Find("__RotatingRaysFX__");
        if (existing != null)
        {
            _rayGo = existing.gameObject;
        }
        else
        {
            _rayGo = new GameObject("__RotatingRaysFX__", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _rayGo.transform.SetParent(transform, false);
            _rayGo.transform.SetAsFirstSibling(); // Đặt ra phía sau nội dung chính
        }

        RectTransform rt = _rayGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Kích thước phủ rộng hơn card
        float maxDim = Mathf.Max(_rectTransform.rect.width, _rectTransform.rect.height);
        if (maxDim <= 0) maxDim = 200f;
        float size = maxDim * 1.5f;
        rt.sizeDelta = new Vector2(size, size);

        _rayImage = _rayGo.GetComponent<Image>();
        _rayImage.sprite = GenerateSunburstSprite(rayCount, raySharpness);
        _rayImage.color = rayColor;
        _rayImage.raycastTarget = false;

        _isBuilt = true;
    }

    private void StartRaysAnimation()
    {
        StopRaysAnimation();

        if (enablePulse && _rayGo != null)
        {
            _pulseTween = _rayGo.transform.DOScale(pulseScaleAmount, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }
    }

    private void StopRaysAnimation()
    {
        if (_pulseTween != null)
        {
            _pulseTween.Kill();
            _pulseTween = null;
        }

        if (_rayGo != null)
        {
            _rayGo.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Sinh Texture2D hình tia sáng tỏa tròn (Sunburst) hoàn toàn trong RAM
    /// </summary>
    private static Sprite _cachedSunburstSprite;
    private static Sprite GenerateSunburstSprite(int count, float sharpness)
    {
        if (_cachedSunburstSprite != null) return _cachedSunburstSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y) - center;
                float dist = pos.magnitude;
                float normDist = Mathf.Clamp01(dist / radius);

                if (normDist >= 1f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                // Tính góc của pixel (-PI đến +PI)
                float angle = Mathf.Atan2(pos.y, pos.x);
                // Tạo các sóng tia sáng đều nhau
                float rayIntensity = Mathf.Cos(angle * count);
                rayIntensity = Mathf.Pow(Mathf.Clamp01((rayIntensity + 1f) * 0.5f), sharpness);

                // Giảm dần độ sáng khi ra mép ngoài (Edge falloff)
                float falloff = Mathf.Pow(1f - normDist, 1.5f);
                float alpha = rayIntensity * falloff;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();

        _cachedSunburstSprite = Sprite.Create(
            tex, 
            new Rect(0, 0, size, size), 
            new Vector2(0.5f, 0.5f), 
            100f
        );

        return _cachedSunburstSprite;
    }
}
