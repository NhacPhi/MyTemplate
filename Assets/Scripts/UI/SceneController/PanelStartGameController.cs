using UnityEngine;
using UIFramework;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using VContainer;

public class PanelStartGameController : PanelController
{
    [SerializeField] private List<NavigationButton> buttons;
    [Inject] private UIManager _uiManager;

    void Start()
    {
        if (buttons != null && buttons.Count > 0)
        {
            foreach (var button in buttons)
            {
                if (button != null && button.button != null)
                {
                    string id = button.StringID();
                    button.button.onClick.AddListener(() => ClickButtonOnPanel(id));
                }
            }
        }
    }

    private void ClickButtonOnPanel(string id)
    {
        if (string.IsNullOrEmpty(id) || _uiManager == null) return;

        _uiManager.HidePanel(ScreenIds.PanelStartGame);

        switch (id)
        {
            case ScreenIds.PopupConfirm:
                Action cancel = () =>
                {
                    _uiManager.ShowPanel(ScreenIds.PanelStartGame);
                };
                Action confirm = () =>
                {
                    Application.Quit();
                };
                ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties(
                    LocalizationManager.Instance.GetLocalizedValue("UI_REMIND"),
                    LocalizationManager.Instance.GetLocalizedValue("UI_QUIT_QUESTION"),
                    LocalizationManager.Instance.GetLocalizedValue("UI_CONFIRM"),
                    LocalizationManager.Instance.GetLocalizedValue("UI_CANCEL"),
                    confirm,
                    cancel
                );
                _uiManager.OpenWindowScene(id, popupProps);
                break;
            default:
                _uiManager.OpenWindowScene(id);
                break;
        }
    }
}
