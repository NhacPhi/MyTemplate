using System.Collections.Generic;
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
    [SerializeField] private Button btnQuitGame;
    [SerializeField] private Button btnSetting;
    [SerializeField] private Button btnRedeemCode;

    [Header("PlayerInfo Info")]
    [SerializeField] private Image avatarIcon;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtPlayerName;
    [SerializeField] private Button btnEditName;
    [SerializeField] private Slider sliderExp;
    [SerializeField] private TextMeshProUGUI txtCurrentExp;

    [Inject] private UIManager uiManager;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private PlayerCharacterManager playerCharacterManager;

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
        if (btnEditName != null) btnEditName.onClick.AddListener(OnEditName);
        if (btnRedeemCode != null) btnRedeemCode.onClick.AddListener(OnRedeemCode);

        btnInventory.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.InventoryScene);
        });

        btnCharacter.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.CharacterScene);
        });

        btnPartySetup.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.PartySetupScene);
            UIEvent.OnPrepareBattleData?.Invoke();
        });

        btnShop.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.ShopScene);
        });

        btnGacha.onClick.AddListener(() =>
        {
            uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.GachaMainScene);
        });

        RegisterQuitGame();
        RegisterSetting();
    }

    private void RegisterQuitGame()
    {
        if (btnQuitGame != null)
        {
            btnQuitGame.onClick.AddListener(() =>
            {
                uiManager.ShowQuitPopup(
                    () => { Application.Quit(); },
                    null
                );
            });
        }
    }

    private void RegisterSetting()
    {
        if (btnSetting != null)
        {
            btnSetting.onClick.AddListener(() =>
            {
                GameSettingsScene.OnCloseAction = () =>
                {
                    uiManager.ShowPanel(ScreenIds.GamePlayPanel);
                };
                uiManager.OpenWindowScene(ScreenIds.GameSettingsScene);
            });
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        if (txtLevel != null && save?.Player?.Account != null)
        {
            txtLevel.text = save.Player.Account.Level.ToString();
        }

        if (save?.Player?.Account != null)
        {
            UpdatePlayerName(save.Player.Account.PlayerName);
            UpdateAvatarIconOnPanel(save.Player.Account.AvatarIcon);
            UpdateExpBar(save.Player.Account.CurrentExp, save.Player.Account.Level);
        }

        UIEvent.OnChanageAvatarPanel += UpdateAvatarIconOnPanel;
    }

    public void UpdateExpBar(int currentExp, int currentLevel)
    {
        int maxExp = currentLevel * 1000;
        SetExp(currentExp, maxExp);
    }

    public void SetExp(int currentExp, int maxExp)
    {
        if (sliderExp != null)
        {
            sliderExp.minValue = 0;
            sliderExp.maxValue = maxExp;
            sliderExp.value = currentExp;
        }

        if (txtCurrentExp != null)
        {
            txtCurrentExp.text = $"{currentExp}/{maxExp}";
        }
    }

    private void OnDisable()
    {
        UIEvent.OnChanageAvatarPanel -= UpdateAvatarIconOnPanel;
    }

    public void OnChangeAvatar()
    {
        uiManager.OpenWindowScene(ScreenIds.PopupChangeAvatar);
    }

    public void OnEditName()
    {
        string title = LocalizationManager.Instance.GetLocalizedValue("UI_EDIT_NAME");
        string placeholder = LocalizationManager.Instance.GetLocalizedValue("UI_ENTER_NEW_NAME");

        string currentName = save.Player.Account.PlayerName;
        if (string.IsNullOrEmpty(currentName)) currentName = "Player";

        uiManager.ShowInputPopup(
            title: title,
            confirmAction: (newName) =>
            {
                if (string.IsNullOrWhiteSpace(newName)) return;

                save.Player.Account.PlayerName = newName.Trim();
                UpdatePlayerName(save.Player.Account.PlayerName);
                save.SaveDataToDisk(GameSaveType.PlayerInfo);
            },
            defaultText: currentName,
            placeholder: placeholder
        );
    }

    public void UpdatePlayerName(string name)
    {
        if (txtPlayerName != null)
        {
            txtPlayerName.text = string.IsNullOrEmpty(name) ? "Player" : name;
        }
    }

    public void UpdateAvatarIconOnPanel(string id)
    {
        avatarIcon.sprite = gameDataBase.GetItemConfig(id).Icon;
    }

    public void OnRedeemCode()
    {
        string title = LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_TITLE");
        string placeholder = LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_PLACEHOLDER");

        uiManager.ShowInputPopup(
            title: title,
            confirmAction: (code) =>
            {
                ProcessRedeemCode(code);
            },
            defaultText: "",
            placeholder: placeholder
        );
    }

    private void ProcessRedeemCode(string inputCode)
    {
        if (string.IsNullOrWhiteSpace(inputCode))
        {
            uiManager.ShowNotification(LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_INVALID"));
            return;
        }

        string trimmedCode = inputCode.Trim();
        RedeemCodeConfig config = gameDataBase.GetRedeemCodeConfig(trimmedCode);

        if (config == null || !config.IsActive)
        {
            uiManager.ShowNotification(LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_INVALID"));
            return;
        }

        if (save.Player.Account.HasClaimedRedeemCode(config.Code))
        {
            uiManager.ShowNotification(LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_USED"));
            return;
        }

        // Mark code as claimed and save to disk
        save.Player.Account.AddClaimedRedeemCode(config.Code);
        save.SaveDataToDisk(GameSaveType.PlayerInfo);

        // Process all rewards from config
        List<RewardItemData> rewardItems = new List<RewardItemData>();

        if (config.Rewards != null)
        {
            foreach (var r in config.Rewards)
            {
                if (string.IsNullOrEmpty(r.Id) || r.Amount <= 0) continue;

                if (string.Equals(r.Type, "Currency", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (System.Enum.TryParse<CurrencyType>(r.Id, true, out var currencyType))
                    {
                        if (currencyMM != null)
                        {
                            currencyMM.Add(currencyType, r.Amount);
                        }
                    }
                }
                else if (string.Equals(r.Type, "Character", System.StringComparison.OrdinalIgnoreCase))
                {
                    save.Player.Roster.AddCharacter(r.Id);
                    UIEvent.OnCharacterAdded?.Invoke(r.Id);
                }
                else if (string.Equals(r.Type, "Weapon", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (inventoryManager != null)
                    {
                        for (int i = 0; i < r.Amount; i++)
                        {
                            inventoryManager.AddWeapon(new WeaponSaveData
                            {
                                UUID = System.Guid.NewGuid().ToString(),
                                TemplateID = r.Id,
                                CurrentLevel = 1
                            });
                        }
                    }
                }
                else
                {
                    if (inventoryManager != null)
                    {
                        inventoryManager.AddStackableItem(r.Id, ItemType.Material, r.Amount);
                    }
                }

                rewardItems.Add(new RewardItemData(r.Id, r.Amount));
            }
        }

        if (rewardItems.Count > 0)
        {
            uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(rewardItems));
        }
        else
        {
            uiManager.ShowNotification(LocalizationManager.Instance.GetLocalizedValue("UI_REDEEM_CODE_SUCCESS"));
        }
    }
}
