using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;

[Serializable]
public class NotificationPopupProperties : WindowProperties
{
    public readonly string title;
    public readonly string txtButtonConfirm;
    public readonly string txtMessage;
    public readonly Action confirmAction;

    public NotificationPopupProperties(string title, string txtMessage, string txtButtonConfirm = "OK", Action confirmAction = null)
    {
        this.title = title;
        this.txtMessage = txtMessage;
        this.txtButtonConfirm = txtButtonConfirm;
        this.confirmAction = confirmAction;

        IsPopup = true;
        HideOnForegroundLost = false;
        SuppressPrefabProperties = true;
        WindowQueuePriority = WindowPriority.ForceForeground;
    }
}

public class NotificationPopupController : WindowController<NotificationPopupProperties>
{
    [SerializeField] public TextMeshProUGUI titleLable;
    [SerializeField] public TextMeshProUGUI txtMessage;
    [SerializeField] public TextMeshProUGUI txtConfirmButton;

    [SerializeField] public Button btnConfirm;

    private void Start()
    {
        if (btnConfirm != null) btnConfirm.onClick.AddListener(() => UI_Confirm());
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (Properties == null) return;

        if (titleLable != null) titleLable.text = Properties.title;
        if (txtMessage != null) txtMessage.text = Properties.txtMessage;
        if (txtConfirmButton != null) txtConfirmButton.text = Properties.txtButtonConfirm;
    }

    public void UI_Confirm()
    {
        UI_Close();
        if (Properties != null && Properties.confirmAction != null)
        {
            Properties.confirmAction();
        }
    }
}
