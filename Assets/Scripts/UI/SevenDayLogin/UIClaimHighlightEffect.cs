using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UIClaimHighlightEffect : MonoBehaviour
{
    [Header("Color & Style")]
    [SerializeField] private Color primaryGlowColor = new Color(1f, 0.88f, 0.2f, 0.9f); // Vàng kim sáng
    [SerializeField] private Color secondaryGlowColor = new Color(1f, 0.55f, 0.1f, 0.6f); // Cam ánh kim
    [SerializeField] private Color sparkleColor = new Color(1f, 1f, 0.8f, 1f); // Trắng vàng lấp lánh
    [SerializeField] private Material customSparkleMaterial; // Optional: SparleMaterial.mat

    [Header("Rotating Rays (Tia sáng xoay tròn nền)")]
    [SerializeField] private bool showRotatingRays = true; // Bật tia sáng xoay tròn
    [SerializeField] private int rayCount = 12; // Số lượng tia
    [SerializeField] private float rayRotationSpeed = 20f; // Tốc độ xoay (độ/giây)
    [SerializeField] private Color rayGlowColor = new Color(1f, 0.85f, 0.3f, 0.35f);

    [Header("Border Settings")]
    [SerializeField] private float borderWidth = 3f;
    [SerializeField] private float padding = 4f; // Độ nới rộng ra ngoài viền card

    [Header("Orbiting Sparkles (Tia sáng chạy quanh viền)")]
    [Range(1, 6)]
    [SerializeField] private int sparkleCount = 2; // Số điểm sáng chạy quanh
    [SerializeField] private float orbitDuration = 3.5f; // Thời gian chạy 1 vòng (giây)
    [SerializeField] private float sparkleSize = 14f;

    [Header("Corner Stars (Sao nhấp nháy ở 4 góc)")]
    [SerializeField] private bool showCornerStars = true;
    [SerializeField] private float cornerStarSize = 16f;

    private RectTransform _rectTransform;
    private GameObject _fxContainer;
    private GameObject _raysGo;
    private List<Image> _borderImages = new List<Image>();
    private List<RectTransform> _sparkleRects = new List<RectTransform>();
    private List<Image> _sparkleImages = new List<Image>();
    private List<RectTransform> _cornerStarRects = new List<RectTransform>();
    private bool _isBuilt = false;
    private bool _isPlaying = false;

    private void Awake()
    {
        EnsureRectTransform();
        BuildProceduralElements();
    }

    private void EnsureRectTransform()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                _rectTransform = gameObject.AddComponent<RectTransform>();
            }
        }
    }

    private void Update()
    {
        if (_isPlaying && _raysGo != null && _raysGo.activeSelf)
        {
            _raysGo.transform.Rotate(0f, 0f, -rayRotationSpeed * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        StopAnimations();
    }

    private void OnDestroy()
    {
        StopAnimations();
    }

    /// <summary>
    /// Bật hiệu ứng chạy quanh viền và tia sáng xoay
    /// </summary>
    public void Play()
    {
        EnsureRectTransform();
        _isPlaying = true;
        if (!_isBuilt)
        {
            BuildProceduralElements();
        }

        if (_fxContainer != null)
        {
            _fxContainer.SetActive(true);
        }

        StartAnimations();
    }

    /// <summary>
    /// Dừng và ẩn toàn bộ hiệu ứng
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        StopAnimations();

        if (_fxContainer != null)
        {
            _fxContainer.SetActive(false);
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (isVisible) Play();
        else Stop();
    }

    /// <summary>
    /// Tự động tạo các thành phần đồ họa hoàn toàn bằng Code (Không cần texture ngoài)
    /// </summary>
    private void BuildProceduralElements()
    {
        EnsureRectTransform();

        if (_isBuilt && _fxContainer != null) return;

        // Tìm hoặc tạo Container riêng chứa FX để không làm ảnh hưởng hierarchy gốc
        var existingFx = transform.Find("__SparkleHighlightFX__");
        if (existingFx != null)
        {
            _fxContainer = existingFx.gameObject;
            _fxContainer.transform.SetAsLastSibling();
        }
        else
        {
            _fxContainer = new GameObject("__SparkleHighlightFX__", typeof(RectTransform));
            _fxContainer.transform.SetParent(transform, false);
            _fxContainer.transform.SetAsLastSibling();

            RectTransform fxRt = _fxContainer.GetComponent<RectTransform>();
            fxRt.anchorMin = Vector2.zero;
            fxRt.anchorMax = Vector2.one;
            fxRt.sizeDelta = Vector2.zero;
            fxRt.anchoredPosition = Vector2.zero;
        }

        _borderImages.Clear();
        _sparkleRects.Clear();
        _sparkleImages.Clear();
        _cornerStarRects.Clear();

        // Xóa các con cũ nếu có
        foreach (Transform child in _fxContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Tạo Sprite tròn mềm procedural từ Texture2D nhỏ
        Sprite circleSprite = CreateSoftCircleSprite();

        // 0. Tạo Rotating Rays ở lớp nền (background)
        if (showRotatingRays)
        {
            _raysGo = new GameObject("RotatingRays", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _raysGo.transform.SetParent(_fxContainer.transform, false);
            _raysGo.transform.SetAsFirstSibling();

            RectTransform rt = _raysGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            float w = _rectTransform != null && _rectTransform.rect.width > 0 ? _rectTransform.rect.width : 180f;
            float h = _rectTransform != null && _rectTransform.rect.height > 0 ? _rectTransform.rect.height : 240f;
            float maxDim = Mathf.Max(w, h);
            float size = maxDim * 1.5f;
            rt.sizeDelta = new Vector2(size, size);

            Image rayImg = _raysGo.GetComponent<Image>();
            rayImg.sprite = GenerateSunburstSprite(rayCount, 2.0f);
            rayImg.color = rayGlowColor;
            rayImg.raycastTarget = false;
        }

        // 1. Tạo 4 đường viền hào quang (Top, Bottom, Left, Right)
        CreateBorderLine("Border_Top", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), circleSprite);
        CreateBorderLine("Border_Bottom", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), circleSprite);
        CreateBorderLine("Border_Left", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), circleSprite);
        CreateBorderLine("Border_Right", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), circleSprite);

        // 2. Tạo các điểm sáng lấp lánh chạy quanh viền (Orbiting Sparkles)
        for (int i = 0; i < sparkleCount; i++)
        {
            GameObject spGo = new GameObject($"Sparkle_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            spGo.transform.SetParent(_fxContainer.transform, false);

            RectTransform rt = spGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(sparkleSize, sparkleSize);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image img = spGo.GetComponent<Image>();
            img.sprite = circleSprite;
            img.color = sparkleColor;
            img.raycastTarget = false;

            if (customSparkleMaterial != null)
            {
                img.material = customSparkleMaterial;
            }

            _sparkleRects.Add(rt);
            _sparkleImages.Add(img);
        }

        // 3. Tạo 4 ngôi sao nhấp nháy ở 4 góc
        if (showCornerStars)
        {
            Vector2[] cornerPivots = new Vector2[]
            {
                new Vector2(0f, 1f), // Top-Left
                new Vector2(1f, 1f), // Top-Right
                new Vector2(1f, 0f), // Bottom-Right
                new Vector2(0f, 0f)  // Bottom-Left
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject starGo = new GameObject($"CornerStar_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starGo.transform.SetParent(_fxContainer.transform, false);

                RectTransform rt = starGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cornerStarSize, cornerStarSize);
                rt.anchorMin = cornerPivots[i];
                rt.anchorMax = cornerPivots[i];
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                Image img = starGo.GetComponent<Image>();
                img.sprite = circleSprite;
                img.color = sparkleColor;
                img.raycastTarget = false;

                _cornerStarRects.Add(rt);
            }
        }

        _isBuilt = true;
        _fxContainer.SetActive(_isPlaying);
    }

    private void CreateBorderLine(string name, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(_fxContainer.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.color = primaryGlowColor;
        img.raycastTarget = false;

        _borderImages.Add(img);
    }

    private void UpdateBorderPositions()
    {
        EnsureRectTransform();

        float baseW = _rectTransform != null && _rectTransform.rect.width > 0 ? _rectTransform.rect.width : 180f;
        float baseH = _rectTransform != null && _rectTransform.rect.height > 0 ? _rectTransform.rect.height : 240f;
        float w = baseW + padding * 2f;
        float h = baseH + padding * 2f;

        if (_borderImages.Count >= 4)
        {
            // Top
            _borderImages[0].rectTransform.sizeDelta = new Vector2(w, borderWidth);
            _borderImages[0].rectTransform.anchoredPosition = new Vector2(0, padding);

            // Bottom
            _borderImages[1].rectTransform.sizeDelta = new Vector2(w, borderWidth);
            _borderImages[1].rectTransform.anchoredPosition = new Vector2(0, -padding);

            // Left
            _borderImages[2].rectTransform.sizeDelta = new Vector2(borderWidth, h);
            _borderImages[2].rectTransform.anchoredPosition = new Vector2(-padding, 0);

            // Right
            _borderImages[3].rectTransform.sizeDelta = new Vector2(borderWidth, h);
            _borderImages[3].rectTransform.anchoredPosition = new Vector2(padding, 0);
        }

        if (_raysGo != null)
        {
            float maxDim = Mathf.Max(baseW, baseH);
            float size = maxDim * 1.5f;
            _raysGo.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        }
    }

    private void StartAnimations()
    {
        EnsureRectTransform();
        StopAnimations();
        UpdateBorderPositions();

        float baseW = _rectTransform != null && _rectTransform.rect.width > 0 ? _rectTransform.rect.width : 180f;
        float baseH = _rectTransform != null && _rectTransform.rect.height > 0 ? _rectTransform.rect.height : 240f;
        float halfW = (baseW / 2f) + padding;
        float halfH = (baseH / 2f) + padding;

        // 0. Hiệu ứng thở nhẹ của Rotating Rays
        if (_raysGo != null)
        {
            _raysGo.transform.DOScale(1.08f, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }

        // 1. Hiệu ứng thở hào quang viền (Border Pulse Breathing)
        foreach (var img in _borderImages)
        {
            if (img == null) continue;
            img.color = primaryGlowColor;

            img.DOColor(secondaryGlowColor, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }

        if (_fxContainer != null)
        {
            _fxContainer.transform.DOScale(1.025f, 0.9f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }

        // 2. Hiệu ứng điểm sáng chạy vòng quanh 4 cạnh (Orbiting Path)
        Vector3[] waypoints = new Vector3[]
        {
            new Vector3(-halfW, halfH, 0),  // 0: Top-Left
            new Vector3(halfW, halfH, 0),   // 1: Top-Right
            new Vector3(halfW, -halfH, 0),  // 2: Bottom-Right
            new Vector3(-halfW, -halfH, 0), // 3: Bottom-Left
            new Vector3(-halfW, halfH, 0)   // 4: Back to Top-Left
        };

        for (int i = 0; i < _sparkleRects.Count; i++)
        {
            RectTransform rt = _sparkleRects[i];
            if (rt == null) continue;

            float offsetProgress = (float)i / _sparkleRects.Count;
            float delay = offsetProgress * orbitDuration;

            rt.anchoredPosition = waypoints[0];

            // Chạy dọc theo chu vi khép kín
            rt.DOPath(waypoints, orbitDuration, PathType.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetDelay(delay)
                .SetId(this);

            // Xoay 360 độ liên tục
            rt.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetId(this);

            // Nhấp nháy lấp lánh (Twinkle scale)
            rt.DOScale(1.35f, 0.4f + (i * 0.1f))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }

        // 3. Hiệu ứng nhấp nháy 4 góc
        for (int i = 0; i < _cornerStarRects.Count; i++)
        {
            RectTransform rt = _cornerStarRects[i];
            if (rt == null) continue;

            rt.localScale = Vector3.one * 0.5f;

            float delay = (i % 2 == 0) ? 0f : 0.5f;
            rt.DOScale(1.3f, 0.7f)
                .SetDelay(delay)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);

            rt.DORotate(new Vector3(0, 0, 180f), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetId(this);
        }
    }

    private void StopAnimations()
    {
        DOTween.Kill(this);
        if (_fxContainer != null)
        {
            _fxContainer.transform.localScale = Vector3.one;
        }
        if (_raysGo != null)
        {
            _raysGo.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Tạo procedural một Sprite hình tròn gradient mềm mịn (radial glow)
    /// </summary>
    private static Sprite _cachedSoftCircleSprite;
    private static Sprite CreateSoftCircleSprite()
    {
        if (_cachedSoftCircleSprite != null) return _cachedSoftCircleSprite;

        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float norm = Mathf.Clamp01(dist / radius);
                float alpha = Mathf.Pow(1f - norm, 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        _cachedSoftCircleSprite = Sprite.Create(
            tex, 
            new Rect(0, 0, size, size), 
            new Vector2(0.5f, 0.5f), 
            100f
        );

        return _cachedSoftCircleSprite;
    }

    /// <summary>
    /// Sinh Texture2D hình tia sáng tỏa tròn (Sunburst Rays)
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

                float angle = Mathf.Atan2(pos.y, pos.x);
                float rayIntensity = Mathf.Cos(angle * count);
                rayIntensity = Mathf.Pow(Mathf.Clamp01((rayIntensity + 1f) * 0.5f), sharpness);

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
