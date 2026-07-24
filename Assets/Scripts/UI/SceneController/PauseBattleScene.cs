using System;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class PauseBattleScene : WindowController
{
    [Inject] private UIManager _uiManager;
    [Inject] private SceneLoader _sceneLoader;
    [Inject] private BattleSessionContext _sessionContext;

    [SerializeField] private Button _btnReturn;
    [SerializeField] private Button _btnRestart;
    [SerializeField] private Button _btnQuit;

    private bool _isExiting = false;

    private void Awake()
    {
        if (_btnReturn != null) _btnReturn.onClick.AddListener(UI_Return);
        if (_btnRestart != null) _btnRestart.onClick.AddListener(UI_Restart);
        if (_btnQuit != null) _btnQuit.onClick.AddListener(UI_Quit);
    }

    private void OnEnable()
    {
        _isExiting = false;
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        if (!_isExiting)
        {
            Time.timeScale = BattleUIScene.CurrentSpeed;
        }
    }

    public void UI_Return()
    {
        UI_Close();
    }

    public void UI_Restart()
    {
        _isExiting = true;
        BattleUIScene.CurrentSpeed = 1f;
        BattleUIScene.IsAutoBattle = false;
        Time.timeScale = 1f;
        UI_Close();
        _sceneLoader.RestartCurrentScene();
    }

    public void UI_Quit()
    {
        Action confirmAction = () =>
        {
            _isExiting = true;
            BattleUIScene.CurrentSpeed = 1f;
            BattleUIScene.IsAutoBattle = false;
            Time.timeScale = 1f;
            UI_Close();
            _uiManager.OpenWindowScene(ScreenIds.GamePlayScene); // Show gameplay panel

            GameSceneSO locToLoad = (_sessionContext != null && _sessionContext.PreviousLocation != null)
                ? _sessionContext.PreviousLocation
                : SceneLoader.LastLoadedLocation;

            if (locToLoad != null)
            {
                _sceneLoader.LoadLocation(locToLoad, true, true);
            }
            else
            {
                Debug.LogError("[PauseBattleScene] PreviousLocation và LastLoadedLocation đều null! Không thể load lại Location.");
            }
        };

        string title =LocalizationManager.Instance.GetLocalizedValue("UI_REMIND");
        string confirmBtn = LocalizationManager.Instance.GetLocalizedValue("UI_CONFIRM");
        string cancelBtn = LocalizationManager.Instance.GetLocalizedValue("UI_CANCEL");
        string content = LocalizationManager.Instance.GetLocalizedValue("STR_ARE_YOU_QUIT_BATTLE");

        ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties(
            title,
            content, 
            confirmBtn, 
            cancelBtn, 
            confirmAction, 
            null
        );

        _uiManager.OpenWindowScene(ScreenIds.PopupConfirm, popupProps);
    }
}
