using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class GamePlayPanel : PanelController
{
    [SerializeField] private Button btnClosePanel;
    [SerializeField] private Button btnChangeAvatar;
    [Header("Player Info")]
    [SerializeField] private Image avatarIcon;

    [Inject] private UIManager uiManager;
    [Inject] private SaveSystem save;
    [Inject] private ItemDataBase itemData;

    private void Start()
    {
        btnClosePanel.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
            uiManager.HidePanel();
            save.SaveDataToDisk(TypeGameSave.Player);
        });

        btnChangeAvatar.onClick.AddListener(OnChangeAvatar);

        UpdateAvatarIconOnPanel(save.Player.AvatarIcon);

        UIEvent.OnChanageAvatarPanel += UpdateAvatarIconOnPanel;
    }

    private void OnDestroy()
    {
        UIEvent.OnChanageAvatarPanel -= UpdateAvatarIconOnPanel;
    }

    public void OnChangeAvatar()
    {
        uiManager.OpenWindowScene(ScreenIds.PopupChangeAvatar);
    }

    public void UpdateAvatarIconOnPanel(string id)
    {
        avatarIcon.sprite = itemData.Avatars.Find(avatar => avatar.ID.Equals(id)).Icon;
    }
}
