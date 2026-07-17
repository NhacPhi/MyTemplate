using VContainer;
using TMPro;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameSettingsScene : WindowController
{
    [SerializeField] private Slider sliderSound;

    [SerializeField] private TextMeshProUGUI txtNumberSound;

    [SerializeField] private TextMeshProUGUI txtFPS;

    [Inject] private SaveSystem save;
    [Inject] private UIManager uiManager;
    [Inject] private IAudioManager audioManager;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnShowFPSSetting;
    [SerializeField] private Button btnShowLangSetting;

    private float _openedRealTime;
    public static System.Action OnCloseAction;

    private void OnEnable()
    {
        _openedRealTime = Time.realtimeSinceStartup;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadGameSettingUI();
        sliderSound.onValueChanged.AddListener((v) => {
            txtNumberSound.text = v.ToString("0");
            save.Settings.MusicVolune = (int)v;
            if (audioManager != null)
            {
                audioManager.UpdateVolume(v / 100f);
            }
        });

        btnClose.onClick.AddListener(() => { OnCloseScene(); });
        btnShowFPSSetting.onClick.AddListener(() => { ShowPopupConfirmSettings(SettingsType.FPS); });
        btnShowLangSetting.onClick.AddListener(() => { ShowPopupConfirmSettings(SettingsType.LANGUAGE); });
    }

    public void OnCloseScene()
    {
        // Chặn đóng cửa sổ nếu click xảy ra trong vòng 1.0s kể từ lúc mở (chống lan truyền click)
        if (Time.realtimeSinceStartup - _openedRealTime < 0.5f)
        {
            Debug.Log("[GameSettingsScene] Ignored close request due to click propagation cooldown.");
            return;
        }

        base.UI_Close();
        save.SaveDataToDisk(GameSaveType.GameSetting);

        if (OnCloseAction != null)
        {
            var callback = OnCloseAction;
            OnCloseAction = null; // Reset callback
            callback.Invoke();
        }
        else
        {
            uiManager.ShowPanel(ScreenIds.PanelStartGame);
        }
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
                return popupProperties = new PopupSettingProperties(() => { LoadGameSettingUI(); },null, SettingsType.FPS, index, Definition.SettingsFPS, "FPS");
            case SettingsType.LANGUAGE:
                int langIndex = 0;
                for(int i = 0; i < Definition.SettingsLanguage.Count; i++)
                {
                    if (Definition.SettingsLanguage[i] == save.Settings.CurrentLocalized)
                    {
                        langIndex = i;
                    }
                }
                List<string> texts = new List<string>();
                foreach(var obj in Definition.SettingsLanguage)
                {
                    texts.Add(LocalizationManager.Instance.GetLocalizedValue("STR_" + obj.ToString()));
                }
                return popupProperties = new PopupSettingProperties(async () => { await LocalizationManager.Instance.LoadLocalizedText(save.Settings.CurrentLocalized);
                    LoadGameSettingUI(); UIEvent.OnLanguageChanged?.Invoke(); }, null, SettingsType.LANGUAGE, langIndex, texts, LocalizationManager.Instance.GetLocalizedValue("STR_LANGUAGE"));
        }
        return popupProperties;
    }
}
