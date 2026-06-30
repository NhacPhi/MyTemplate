using UnityEngine.UI;
using TMPro;
using UIFramework;
using UnityEngine;

public class LoadingSceneToScene : WindowController
{
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Header("Moving Image Settings")]
    [SerializeField] private RectTransform _movingImage;
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _endPos;

    [Header("Background Settings")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Sprite[] _backgroundSprites; // Gán 4 sprites background vào đây trên Inspector

    private void OnEnable()
    {
        UIEvent.OnUpdateLoadingProgress += UpdateProgress;
        UpdateProgress(0f);

        // Đổi background ngẫu nhiên mỗi lần bật Loading Screen
        if (_backgroundImage != null && _backgroundSprites != null && _backgroundSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, _backgroundSprites.Length);
            _backgroundImage.sprite = _backgroundSprites[randomIndex];
        }
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateLoadingProgress -= UpdateProgress;
    }

    public void UpdateProgress(float value)
    {
        if (_progressBar != null)
        {
            // Tự động scale theo minValue và maxValue của Slider
            _progressBar.value = Mathf.Lerp(_progressBar.minValue, _progressBar.maxValue, value);
        }

        if (_loadingText != null)
        {
            string loadingLabel = LocalizationManager.Instance.GetLocalizedValue("STR_LOADING");
            _loadingText.text = $"{loadingLabel} {(value * 100):F0}%";
        }

        if (_movingImage != null && _startPos != null && _endPos != null)
        {
            _movingImage.position = Vector3.Lerp(_startPos.position, _endPos.position, value);
        }
    }
}
