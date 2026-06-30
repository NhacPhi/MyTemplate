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

    private void Awake()
    {
        if (_btnReturn != null) _btnReturn.onClick.AddListener(UI_Return);
        if (_btnRestart != null) _btnRestart.onClick.AddListener(UI_Restart);
        if (_btnQuit != null) _btnQuit.onClick.AddListener(UI_Quit);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = BattleUIScene.CurrentSpeed;
    }

    public void UI_Return()
    {
        UI_Close();
    }

    public void UI_Restart()
    {
        UI_Close();
        _sceneLoader.RestartCurrentScene();
    }

    public void UI_Quit()
    {
        Action confirmAction = () =>
        {
            UI_Close();
            _uiManager.OpenWindowScene(ScreenIds.GamePlayScene); // Show gameplay panel
            if (_sessionContext != null && _sessionContext.PreviousLocation != null)
            {
                _sceneLoader.LoadLocation(_sessionContext.PreviousLocation, true, true);
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
