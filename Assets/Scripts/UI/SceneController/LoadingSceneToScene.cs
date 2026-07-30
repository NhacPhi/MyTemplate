using UnityEngine.UI;
using TMPro;
using UIFramework;
using UnityEngine;

using System.Collections.Generic;

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

    private static readonly List<int> s_availableIndices = new List<int>();
    private static int s_lastSelectedIndex = -1;

    private void OnEnable()
    {
        UIEvent.OnUpdateLoadingProgress += UpdateProgress;
        UpdateProgress(0f);

        // Đổi background ngẫu nhiên không lặp lại trong 4 lần
        SetRandomBackground();
    }

    private void SetRandomBackground()
    {
        if (_backgroundImage == null || _backgroundSprites == null || _backgroundSprites.Length == 0)
            return;

        // Nếu danh sách khả dụng rỗng hoặc số lượng sprite đã thay đổi, khởi tạo lại pool index
        if (s_availableIndices.Count == 0 || s_availableIndices.Count > _backgroundSprites.Length)
        {
            s_availableIndices.Clear();
            for (int i = 0; i < _backgroundSprites.Length; i++)
            {
                s_availableIndices.Add(i);
            }

            // Trộn ngẫu nhiên danh sách (Fisher-Yates Shuffle)
            for (int i = 0; i < s_availableIndices.Count; i++)
            {
                int randomIndex = Random.Range(i, s_availableIndices.Count);
                int temp = s_availableIndices[i];
                s_availableIndices[i] = s_availableIndices[randomIndex];
                s_availableIndices[randomIndex] = temp;
            }

            // Đảm bảo ảnh đầu tiên của chu kỳ mới không trùng với ảnh cuối cùng của chu kỳ trước
            if (s_availableIndices.Count > 1 && s_availableIndices[0] == s_lastSelectedIndex)
            {
                int lastIdxInList = s_availableIndices.Count - 1;
                int temp = s_availableIndices[0];
                s_availableIndices[0] = s_availableIndices[lastIdxInList];
                s_availableIndices[lastIdxInList] = temp;
            }
        }

        s_lastSelectedIndex = s_availableIndices[0];
        s_availableIndices.RemoveAt(0);

        _backgroundImage.sprite = _backgroundSprites[s_lastSelectedIndex];
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
