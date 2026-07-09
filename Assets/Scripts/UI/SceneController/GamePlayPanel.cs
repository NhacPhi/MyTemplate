using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

public class GamePlayPanel : PanelController
{
    [SerializeField] private Button btnClosePanel;
    [SerializeField] private Button btnChangeAvatar;

    [SerializeField] private Button btnShop;
    [SerializeField] private Button btnGacha;

    [SerializeField] private Button btnInventory;
    [SerializeField] private Button btnCharacter;

    [SerializeField] private Button btnPartySetup;
    [Header("PlayerInfo Info")]
    [SerializeField] private Image avatarIcon;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [Inject] private UIManager uiManager;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private CurrencyManager currencyMM;

    private void Start()
    {
        btnClosePanel.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            UIEvent.OnToggleGamePlayScene?.Invoke(true);
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            save.SaveDataToDisk(GameSaveType.PlayerInfo);
        });

        btnChangeAvatar.onClick.AddListener(OnChangeAvatar);

        btnInventory.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.InventoryScene);
        });

        btnCharacter.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.CharacterScene);
        });

        btnPartySetup.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.PartySetupScene);
            UIEvent.OnPrepareBattleData?.Invoke();
        });

        btnShop.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.ShopScene);
        });

        btnGacha.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.GachaMainScene);
        });

    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        txtLevel.text = save.Player.Account.Level.ToString();

        UpdateAvatarIconOnPanel(save.Player.Account.AvatarIcon);

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
        avatarIcon.sprite = gameDataBase.GetItemConfig(id).Icon;
    }   
}
