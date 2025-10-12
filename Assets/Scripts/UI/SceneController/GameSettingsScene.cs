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
    // Start is called before the first frame update
    void Start()
    {
        LoadGameSettingUI();
        sliderSound.onValueChanged.AddListener((v) => {
            txtNumberSound.text = v.ToString("0");
            //changeMusicVolume.RaiseEvent((int)v);
        });
        btnClose.onClick.AddListener(() => { OnCloseScene(); });
    }

    public void OnCloseScene()
    {
        base.UI_Close();
        save.SaveDataToDisk();
        uiManager.ShowPanel(ScreenIds.PanelStartGame);
    }
    
    private void LoadGameSettingUI()
    {
        txtFPS.text = save.Settings.FPS.ToString();
        txtNumberSound.text = save.Settings.MusicVolune.ToString();
        sliderSound.value = save.Settings.MusicVolune;
    }
}
