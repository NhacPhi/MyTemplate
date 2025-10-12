using System;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using UnityEngine;


[Serializable]
public class ConfirmationPopupProperties : WindowProperties
{
    public readonly string title;
    public readonly string txtButtonConfirm;
    public readonly string txtButtonCancel;
    public readonly string txtMessage;

    public readonly Action confirmAction;
    public readonly Action cancelAction;

    public ConfirmationPopupProperties(string title, string txtMessage, string txtButtonConfirm = "Confirm", string txtButtonCancel = "Cancel", Action confirmAction = null, Action cancelAction = null)
    {
        this.title = title;
        this.txtMessage = txtMessage;
        this.txtButtonConfirm = txtButtonConfirm;
        this.txtButtonCancel = txtButtonCancel;
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;
    }
}

public class ConfirmPopupController : WindowController<ConfirmationPopupProperties>
{
    [SerializeField] public TextMeshProUGUI titleLable;
    [SerializeField] public TextMeshProUGUI txtMessage;
    [SerializeField] public TextMeshProUGUI txtConfirmButton;
    [SerializeField] public TextMeshProUGUI txtCancelButton;

    [SerializeField] public Button btnConfirm;
    [SerializeField] public Button btnCancel;

    private void Awake()
    {
        btnConfirm.onClick.AddListener(() => UI_Confirm());
        btnCancel.onClick.AddListener(() => UI_Cancel());
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();

        titleLable.text = Properties.title;
        txtMessage.text = Properties.txtMessage;
        txtCancelButton.text = Properties.txtButtonCancel;
        txtConfirmButton.text = Properties.txtButtonConfirm;
    }

    public void UI_Confirm()
    {
        UI_Close();
        if (Properties.confirmAction != null)
        {
            Properties.confirmAction();
        }
    }

    public void UI_Cancel()
    {
        UI_Close();
        if (Properties.cancelAction != null)
        {
            Properties.cancelAction();
        }
    }
}
