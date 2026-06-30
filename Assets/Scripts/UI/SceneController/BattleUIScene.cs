using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

public class BattleUIScene : WindowController
{
    [Inject] private UIManager _uiManager;
    [SerializeField] private Button _btnPause;
    [SerializeField] private Button _btnFastForward;
    [SerializeField] private TextMeshProUGUI _txtFastForward;

    [SerializeField] private Button _btnAuto;
    [SerializeField] private TextMeshProUGUI _txtAuto;
    [SerializeField] private Sprite _iconAutoOn;
    [SerializeField] private Sprite _iconAutoOff;

    public static float CurrentSpeed = 1f;
    public static bool IsAutoBattle = false;

    private void Awake()
    {
        if (_btnPause != null)
        {
            _btnPause.onClick.AddListener(OnPauseClicked);
        }
        if (_btnFastForward != null)
        {
            _btnFastForward.onClick.AddListener(OnFastForwardClicked);
        }
        if (_btnAuto != null)
        {
            _btnAuto.onClick.AddListener(OnAutoClicked);
        }
    }

    private void OnEnable()
    {
        CurrentSpeed = 1f;
        IsAutoBattle = false;
        Time.timeScale = CurrentSpeed;
        UpdateFastForwardText();
        UpdateAutoUI();
    }

    private void OnAutoClicked()
    {
        IsAutoBattle = !IsAutoBattle;
        UpdateAutoUI();
    }

    private void UpdateAutoUI()
    {
        if (_txtAuto != null)
        {
            _txtAuto.text = IsAutoBattle ? "Auto" : "Off";
        }

        if (_btnAuto != null && _btnAuto.image != null)
        {
            if (IsAutoBattle && _iconAutoOn != null)
                _btnAuto.image.sprite = _iconAutoOn;
            else if (!IsAutoBattle && _iconAutoOff != null)
                _btnAuto.image.sprite = _iconAutoOff;
        }
    }

    private void OnFastForwardClicked()
    {
        if (CurrentSpeed == 1f)
            CurrentSpeed = 1.5f;
        else if (CurrentSpeed == 1.5f)
            CurrentSpeed = 2f;
        else
            CurrentSpeed = 1f;

        Time.timeScale = CurrentSpeed;
        UpdateFastForwardText();
    }

    private void UpdateFastForwardText()
    {
        if (_txtFastForward != null)
        {
            _txtFastForward.text = $"x{CurrentSpeed}";
        }
    }

    private void OnPauseClicked()
    {
        _uiManager.OpenWindowScene(ScreenIds.PauseBattleScene);
    }
}
