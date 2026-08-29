using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;

[Serializable]
public class PopupInputProperties : WindowProperties
{
    public readonly string title;
    public readonly string defaultText;
    public readonly string placeholderText;
    public readonly string txtButtonConfirm;
    public readonly string txtButtonCancel;

    public readonly Action<string> confirmAction;
    public readonly Action cancelAction;

    public PopupInputProperties(
        string title,
        string defaultText = "",
        string placeholderText = "Enter text...",
        string txtButtonConfirm = "Confirm",
        string txtButtonCancel = "Cancel",
        Action<string> confirmAction = null,
        Action cancelAction = null)
    {
        this.title = title;
        this.defaultText = defaultText;
        this.placeholderText = placeholderText;
        this.txtButtonConfirm = txtButtonConfirm;
        this.txtButtonCancel = txtButtonCancel;
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;

        IsPopup = true;
        HideOnForegroundLost = false;
        SuppressPrefabProperties = true;
        WindowQueuePriority = WindowPriority.ForceForeground;
    }
}

public class PopupInputController : WindowController<PopupInputProperties>
{
    [Header("Main UI Elements")]
    [SerializeField] public TextMeshProUGUI txtTitle;
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] public Button btnConfirm;
    [SerializeField] public Button btnCancel;

    [Header("Optional UI Elements")]
    [SerializeField] public Button btnClose;
    [SerializeField] public TextMeshProUGUI txtConfirmButton;
    [SerializeField] public TextMeshProUGUI txtCancelButton;

    // Aliases for spelling variations
    public TextMeshProUGUI txtTile => txtTitle;
    public Button btnComfirm => btnConfirm;

    private void Start()
    {
        if (btnConfirm != null) btnConfirm.onClick.AddListener(UI_Confirm);
        if (btnCancel != null) btnCancel.onClick.AddListener(UI_Cancel);
        if (btnClose != null) btnClose.onClick.AddListener(UI_Cancel);

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(_ => UI_Confirm());
        }

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

        if (txtTitle != null)
        {
            txtTitle.text = Properties.title;
        }

        if (inputField != null)
        {
            inputField.text = Properties.defaultText ?? string.Empty;

            if (inputField.placeholder is TextMeshProUGUI placeholderText)
            {
                placeholderText.text = Properties.placeholderText ?? "Enter text...";
            }

            inputField.ActivateInputField();
        }

        bool hasCancelButton = !string.IsNullOrEmpty(Properties.txtButtonCancel);
        if (btnCancel != null) btnCancel.gameObject.SetActive(hasCancelButton);
        if (txtCancelButton != null && hasCancelButton) txtCancelButton.text = Properties.txtButtonCancel;

        if (txtConfirmButton != null)
        {
            txtConfirmButton.text = Properties.txtButtonConfirm ?? "Confirm";
        }
    }

    public void UI_Confirm()
    {
        string text = inputField != null ? inputField.text : string.Empty;
        UI_Close();

        Properties?.confirmAction?.Invoke(text);
    }

    public void UI_Cancel()
    {
        UI_Close();

        Properties?.cancelAction?.Invoke();
    }
}
