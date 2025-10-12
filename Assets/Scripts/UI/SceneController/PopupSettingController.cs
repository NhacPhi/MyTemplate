using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UIFramework;
using System.Collections.Generic;
using VContainer;

[Serializable]
public class SettingOption
{
    public Button btn;
    public TextMeshProUGUI info;
    public Toggle radio;
}

public enum SettingsType
{
    none = 0,
    FPS,
    LANGUAGE
}
[Serializable]
public class PopupSettingProperties : WindowProperties
{
    public readonly Action confirmAction;
    public readonly Action cancelAction;

    public SettingsType type;
    public int index;

    public List<string> content;

    public PopupSettingProperties(Action confirmAction, Action cancelAction, SettingsType type, int index, List<string> content)
    {
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;
        this.type = type;
        this.index = index;
        this.content = content;
    }
}
public class PopupSettingController : WindowController<PopupSettingProperties>
{
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;
    private SettingsType type = 0;
    private int currentIndex = 0;
    private List<string> content;
    [SerializeField] public List<SettingOption> options;

    [Inject] private SaveSystem save;

    private void Start()
    {
        btnCancel.onClick.AddListener(() => { UI_Cancel(); });
        btnConfirm.onClick.AddListener(() => { UI_Confirm(); });
    }
    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        this.type = Properties.type;
        this.currentIndex = Properties.index;
        this.content = Properties.content;

        for(int i = 0; i < options.Count; i++)
        {
            options[i].btn.gameObject.SetActive(true);
            options[i].info.text = content[i];
            Toggle radio = options[i].radio;

            int index = i;
            options[i].btn.onClick.AddListener(() => { 
                radio.isOn = true;
                currentIndex = index;

            });

            if(i == Properties.index)
            {
                options[i].radio.isOn = true;
            }
        }
    }

    public void UI_Confirm()
    {
        UI_Close();

        switch(type)
        {
            case SettingsType.FPS:
                save.Settings.FPS = int.Parse(Definition.SettingsFPS[currentIndex]);
                break;
            case SettingsType.LANGUAGE:
                save.Settings.CurrentLocalized = Definition.SettingsLanguage[currentIndex];
                break;
        }

        if (Properties.confirmAction != null)
        {
            Properties.confirmAction();
        }
    }

    public void UI_Cancel()
    {
        UI_Close();

        foreach (var option in options)
        {
            option.btn.gameObject.SetActive(false);
            option.radio.isOn = false;
        }

        if (Properties.cancelAction != null)
        {
            Properties.cancelAction();
        }
    }
}
