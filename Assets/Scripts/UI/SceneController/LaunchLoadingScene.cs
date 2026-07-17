using UnityEngine.UI;
using TMPro;
using UIFramework;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

public class LaunchLoadingScene : WindowController
{
    [Inject] private IAudioManager _audioManager;

    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Header("Moving Image Settings")]
    [SerializeField] private RectTransform _movingImage;
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _endPos;

    private void OnEnable()
    {
        UIEvent.OnUpdateLoadingProgress += UpdateProgress;

        if (_audioManager != null)
        {
            _audioManager.PlaySFXAsync("AudioDB_Environment", true, true).Forget();
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
            // Tự động scale theo minValue và maxValue của Slider (phòng trường hợp max = 100)
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
