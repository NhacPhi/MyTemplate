using UnityEngine.UI;
using TMPro;
using UIFramework;
using UnityEngine;

public class LoadingScreen : WindowController
{
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    public void UpdateProgress(float value)
    {
        _progressBar.value = value;
        string loadingLabel = LocalizationManager.Instance.GetLocalizedValue("STR_LOADING");
        _loadingText.text = $"{loadingLabel} {(value * 100):F0}%";
    }
}
