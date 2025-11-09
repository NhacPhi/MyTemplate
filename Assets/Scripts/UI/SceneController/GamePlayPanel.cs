using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;
using VContainer;
public class GamePlayPanel : PanelController
{
    [SerializeField] private Button btnClosePanel;
    [SerializeField] private Button btnChangeAvatar;
    [Header("PlayerInfo Info")]
    [SerializeField] private Image avatarIcon;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [Inject] private UIManager uiManager;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    private void Start()
    {
        btnClosePanel.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
            uiManager.HidePanel();
            save.SaveDataToDisk(GameSaveType.PlayerInfo);
        });

        btnChangeAvatar.onClick.AddListener(OnChangeAvatar);
    }

    private void OnEnable()
    {
        txtLevel.text = save.Player.Level.ToString();

        UpdateAvatarIconOnPanel(save.Player.AvatarIcon);

        UIEvent.OnChanageAvatarPanel += UpdateAvatarIconOnPanel;
    }

    private void OnDisable()
    {
        UIEvent.OnChanageAvatarPanel -= UpdateAvatarIconOnPanel;
    }

    public void OnChangeAvatar()
    {
        uiManager.OpenWindowScene(ScreenIds.PopupChangeAvatar);
    }

    public void UpdateAvatarIconOnPanel(string id)
    {
        avatarIcon.sprite = gameDataBase.GetItemSOByID<AvatarIconSO>(ItemType.Avatar, id).Icon;
    }   
}
