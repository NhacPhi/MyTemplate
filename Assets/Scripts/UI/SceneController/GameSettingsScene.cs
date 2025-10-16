using VContainer;
using TMPro;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsScene : WindowController
{
    [SerializeField] private Slider sliderSound;

    [SerializeField] private TextMeshProUGUI txtNumberSound;

    [SerializeField] private TextMeshProUGUI txtFPS;

    [Inject] private SaveSystem save;
    [Inject] private UIManager uiManager;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnShowFPSSetting;
    [SerializeField] private Button btnShowLangSetting;


    // Start is called before the first frame update
    void Start()
    {
        LoadGameSettingUI();
        sliderSound.onValueChanged.AddListener((v) => {
            txtNumberSound.text = v.ToString("0");
            save.Settings.MusicVolune = (int)v;
        });

        btnClose.onClick.AddListener(() => { OnCloseScene(); });
        btnShowFPSSetting.onClick.AddListener(() => { ShowPopupConfirmSettings(SettingsType.FPS); });
        btnShowLangSetting.onClick.AddListener(() => { ShowPopupConfirmSettings(SettingsType.LANGUAGE); });
    }

    public void OnCloseScene()
    {
        base.UI_Close();
        save.SaveDataToDisk(GameSaveType.GameSetting);
        uiManager.ShowPanel(ScreenIds.PanelStartGame);
    }
    
    private void LoadGameSettingUI()
    {
        txtFPS.text = save.Settings.FPS.ToString();
        txtNumberSound.text = save.Settings.MusicVolune.ToString();
        sliderSound.value = save.Settings.MusicVolune;
    }

    public void ShowPopupConfirmSettings(SettingsType type)
    {
        uiManager.ShowPopupConfirmSettings(GetPopupData(type));
    }

    private PopupSettingProperties GetPopupData(SettingsType type)
    {
        PopupSettingProperties popupProperties = null;
        switch(type)
        {
            case SettingsType.FPS:
                int index = 0;
                for (int i = 0; i < Definition.SettingsFPS.Count; i++)
                {
                    if (int.Parse(Definition.SettingsFPS[i]) == save.Settings.FPS)
                    {
                        index = i;
                    }
                }
                return popupProperties = new PopupSettingProperties(() => { LoadGameSettingUI(); },null, SettingsType.FPS, index, Definition.SettingsFPS);
            case SettingsType.LANGUAGE:
                int langIndex = 0;
                for(int i = 0; i < Definition.SettingsLanguage.Count; i++)
                {
                    if (Definition.SettingsLanguage[i] == save.Settings.CurrentLocalized)
                    {
                        langIndex = i;
                    }
                }
                return popupProperties = new PopupSettingProperties(async () => { await LocalizationManager.Instance.LoadLocalizedText(save.Settings.CurrentLocalized);
                    LoadGameSettingUI(); UIEvent.OnLanguageChanged?.Invoke(); }, null, SettingsType.LANGUAGE, langIndex, Definition.SettingsLanguage);
        }
        return popupProperties;
    }
}
