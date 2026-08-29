using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class SevenDayLoginScene : WindowController
{
    [Header("UI General")]
    [SerializeField] private Button btnClose;

    [Header("Item Prefabs (Quản lý các Prefab dùng chung)")]
    [SerializeField] private GameItemUI gameItemPrefab;
    [SerializeField] private CharacterIconUI characterIconPrefab;
    [SerializeField] private WeaponUI weaponPrefab;

    [Header("Day Reward Cards (Tạo động từ Prefab)")]
    [Tooltip("Container (Transform/Grid) nơi các thẻ ngày 1 -> 6 được sinh ra")]
    [SerializeField] private Transform dayCardsContainer;

    [Tooltip("Prefab duy nhất của thẻ ngày SevenDayLoginUI")]
    [SerializeField] private SevenDayLoginUI sevenDayCardPrefab;

    [Header("Special Day 7 Card")]
    [SerializeField] private SevenDayLoginCharacterTargetUI day7Card;

    [Header("Configurable Rewards (Optional)")]
    [SerializeField] private List<SevenDayRewardItem> customRewards = new List<SevenDayRewardItem>();

    [Inject] private UIManager uiManager;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private CurrencyManager currencyManager;
    [Inject] private PlayerCharacterManager characterManager;
    [Inject] private SaveSystem saveSystem;

    private int currentLoginDay = 1;
    private HashSet<int> claimedDays = new HashSet<int>();
    private string selectedDay7CharacterId = "";
    private List<SevenDayLoginUI> _spawnedDayCards = new List<SevenDayLoginUI>();

    private void Awake()
    {
        AutoWire();
        LoadFromSaveData();
    }

    private void Start()
    {
        AutoWire();
        LoadFromSaveData();

        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClose);
        }

        RefreshSevenDayUI();
    }

    private void OnEnable()
    {
        AutoWire();
        LoadFromSaveData();
        RefreshSevenDayUI();
    }

    private void OnDestroy()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(OnClose);
        }
    }

    private void LoadFromSaveData()
    {
        var save = saveSystem ?? SaveSystem.Instance;
        if (save != null && save.Player != null)
        {
            if (save.Player.SevenDayLogin == null)
            {
                save.Player.GetType().GetProperty("SevenDayLogin")?.SetValue(save.Player, new SevenDayLoginSaveData());
            }

            var sevenDayData = save.Player.SevenDayLogin;
            if (sevenDayData != null)
            {
                if (!string.IsNullOrEmpty(sevenDayData.SelectedDay7CharacterId))
                {
                    selectedDay7CharacterId = sevenDayData.SelectedDay7CharacterId;
                }
                if (sevenDayData.ClaimedDays != null)
                {
                    claimedDays = new HashSet<int>(sevenDayData.ClaimedDays);
                }

                // Cập nhật ngày đăng nhập theo ngày thực tế (Real-time date check)
                string todayStr = System.DateTime.Now.ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(sevenDayData.LastLoginDate))
                {
                    sevenDayData.LastLoginDate = todayStr;
                    sevenDayData.CurrentLoginDay = 1;
                    save.SaveDataToDisk(GameSaveType.PlayerInfo);
                }
                else if (sevenDayData.LastLoginDate != todayStr)
                {
                    // Sang ngày mới -> Tăng ngày đăng nhập tích lũy (tối đa 7 ngày)
                    sevenDayData.LastLoginDate = todayStr;
                    sevenDayData.CurrentLoginDay = Mathf.Clamp(sevenDayData.CurrentLoginDay + 1, 1, 7);
                    save.SaveDataToDisk(GameSaveType.PlayerInfo);
                }

                currentLoginDay = Mathf.Clamp(sevenDayData.CurrentLoginDay, 1, 7);
            }
        }
    }

    private void AutoWire()
    {
        if (dayCardsContainer == null)
        {
            dayCardsContainer = transform.Find("Content");
        }

        if (day7Card == null)
        {
            day7Card = GetComponentInChildren<SevenDayLoginCharacterTargetUI>(true);
        }

        if (btnClose == null)
        {
            var btnT = transform.Find("btnClose");
            if (btnT != null) btnClose = btnT.GetComponent<Button>();
        }
    }

    /// <summary>
    /// Làm mới và gán dữ liệu cho toàn bộ 7 ngày
    /// </summary>
    public void RefreshSevenDayUI()
    {
        AutoWire();

        // 1. Khởi tạo / Sinh động các thẻ ngày 1 -> 6 từ Prefab duy nhất dựa trên Data Config
        SpawnDayCardsIfNeeded();

        for (int i = 0; i < _spawnedDayCards.Count; i++)
        {
            int dayNumber = i + 1;
            SevenDayLoginUI card = _spawnedDayCards[i];
            if (card == null) continue;

            card.InitDependencies(gameDataBase);

            SevenDayRewardState state = GetDayRewardState(dayNumber);
            SevenDayRewardItem itemData = GetRewardConfigForDay(dayNumber, state);

            card.Setup(itemData, OnClaimRewardClicked, gameDataBase, gameItemPrefab, characterIconPrefab, weaponPrefab);
        }

        // 2. Khởi tạo / Cập nhật ngày 7
        if (day7Card != null)
        {
            day7Card.InitDependencies(uiManager, gameDataBase);
            SevenDayRewardState day7State = GetDayRewardState(7);

            List<string> selectableHeroes = GetSelectableDay7Heroes();

            day7Card.Setup(
                7,
                selectedDay7CharacterId,
                selectableHeroes,
                day7State,
                OnDay7CharacterSelected,
                OnClaimDay7Character,
                uiManager,
                gameDataBase
            );
        }
    }

    private List<SevenDayLoginConfigData> GetSevenDayConfigsList()
    {
        if (gameDataBase != null)
        {
            var configs = gameDataBase.GetSevenDayLoginConfigs();
            if (configs != null && configs.Count > 0) return configs;
        }

        // Fallback đọc trực tiếp file SevenDayLoginConfig.json
        try
        {
            string path = System.IO.Path.Combine(Application.dataPath, "Data/GameConfig/SevenDayLoginConfig.json");
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                var loaded = Tech.Json.Json.DeserializeObject<List<SevenDayLoginConfigData>>(json);
                if (loaded != null && loaded.Count > 0) return loaded;
            }
        }
        catch { }

        return null;
    }

    private void SpawnDayCardsIfNeeded()
    {
        var configs = GetSevenDayConfigsList();
        int targetCount = 6;
        if (configs != null && configs.Count > 0)
        {
            targetCount = configs.Count;
            // Nếu ngày cuối cùng là Ngày 7 (dành cho day7Card) thì trừ đi 1
            if (configs.Exists(c => c.DayNumber >= 7))
            {
                targetCount = configs.FindAll(c => c.DayNumber < 7).Count;
            }
        }

        if (_spawnedDayCards.Count == targetCount && _spawnedDayCards.Count > 0) return;

        // Nếu có container và prefab -> Xóa cũ và sinh mới các ngày từ Data Config
        if (dayCardsContainer != null)
        {
            if (sevenDayCardPrefab != null)
            {
                foreach (Transform child in dayCardsContainer)
                {
                    Destroy(child.gameObject);
                }
                _spawnedDayCards.Clear();

                if (configs != null && configs.Count > 0)
                {
                    foreach (var cfg in configs)
                    {
                        if (cfg.DayNumber >= 7) continue; // Ngày 7 quản lý bởi day7Card
                        SevenDayLoginUI card = Instantiate(sevenDayCardPrefab, dayCardsContainer);
                        card.name = $"ItemUI_Day0{cfg.DayNumber}";
                        _spawnedDayCards.Add(card);
                    }
                }
                else
                {
                    for (int day = 1; day <= targetCount; day++)
                    {
                        SevenDayLoginUI card = Instantiate(sevenDayCardPrefab, dayCardsContainer);
                        card.name = $"ItemUI_Day0{day}";
                        _spawnedDayCards.Add(card);
                    }
                }
            }
            else if (_spawnedDayCards.Count == 0)
            {
                // Fallback nếu không gán prefab: Tự động lấy các card có sẵn trong container
                var existingCards = dayCardsContainer.GetComponentsInChildren<SevenDayLoginUI>(true);
                _spawnedDayCards.AddRange(existingCards);
            }
        }
    }

    private SevenDayRewardState GetDayRewardState(int dayNumber)
    {
        if (claimedDays.Contains(dayNumber))
        {
            return SevenDayRewardState.Claimed; // Đã nhận
        }

        if (dayNumber <= currentLoginDay)
        {
            return SevenDayRewardState.CanClaim; // Mở khóa nhận theo ngày đăng nhập
        }

        return SevenDayRewardState.Locked; // Chưa tới ngày mở khóa
    }

    private SevenDayRewardItem GetRewardConfigForDay(int dayNumber, SevenDayRewardState state)
    {
        // 1. Nếu có config tùy chỉnh trong inspector
        if (customRewards != null && customRewards.Count >= dayNumber)
        {
            var cfg = customRewards[dayNumber - 1];
            cfg.State = state;
            return cfg;
        }

        // 2. Lấy từ Config Data (Shop.xlsx -> SevenDayLoginConfig.json)
        var allConfigs = GetSevenDayConfigsList();
        if (allConfigs != null)
        {
            var dbConfig = allConfigs.Find(c => c.DayNumber == dayNumber);
            if (dbConfig != null)
            {
                if (System.Enum.TryParse<SevenDayRewardType>(dbConfig.RewardType, true, out var rType))
                {
                    return new SevenDayRewardItem(dayNumber, rType, dbConfig.RewardId, dbConfig.Amount, state, dbConfig.CustomName);
                }
                return new SevenDayRewardItem(dayNumber, SevenDayRewardType.Item, dbConfig.RewardId, dbConfig.Amount, state, dbConfig.CustomName);
            }
        }

        // 3. Mẫu mặc định fallback
        switch (dayNumber)
        {
            case 1:
                return new SevenDayRewardItem(1, SevenDayRewardType.Currency, "Jade", 200, state);
            case 2:
                return new SevenDayRewardItem(2, SevenDayRewardType.Character, "ErlangShen", 1, state);
            case 3:
                return new SevenDayRewardItem(3, SevenDayRewardType.Currency, "Ticket", 10, state);
            case 4:
                return new SevenDayRewardItem(4, SevenDayRewardType.Currency, "RelicEssence", 5000, state);
            case 5:
                return new SevenDayRewardItem(5, SevenDayRewardType.Weapon, "Triple_Edged_Blade", 1, state);
            case 6:
                return new SevenDayRewardItem(6, SevenDayRewardType.Item, "supreme_exp", 10, state);
            default:
                return new SevenDayRewardItem(dayNumber, SevenDayRewardType.Item, "Coin", 10000, state);
        }
    }

    private List<string> GetSelectableDay7Heroes()
    {
        // 1. Đọc từ Data Config nếu có định nghĩa danh sách ID tướng ở ngày 7
        var allConfigs = GetSevenDayConfigsList();
        if (allConfigs != null)
        {
            var day7Cfg = allConfigs.Find(c => c.DayNumber == 7);
            if (day7Cfg != null && !string.IsNullOrEmpty(day7Cfg.RewardId))
            {
                string[] split = day7Cfg.RewardId.Split(new[] { ',', '|', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 0)
                {
                    List<string> list = new List<string>();
                    foreach (var s in split) list.Add(s.Trim());
                    return list;
                }
            }
        }

        // 2. Mặc định các danh sách tướng SSR/UR trong GameDataBase
        return new List<string>
        {
            "ErlangShen",
            "SunWukong",
            "ThirdPrinceNezha",
            "BullDemonKing"
        };
    }

    private void OnDay7CharacterSelected(string characterId)
    {
        selectedDay7CharacterId = characterId;
        Debug.Log($"[SevenDayLoginScene] Day 7 target character selected: {characterId}");

        var save = saveSystem ?? SaveSystem.Instance;
        if (save != null && save.Player?.SevenDayLogin != null)
        {
            save.Player.SevenDayLogin.SelectedDay7CharacterId = characterId;
            save.SaveDataToDisk(GameSaveType.PlayerInfo);
        }
    }

    private void OnClaimRewardClicked(int dayNumber)
    {
        Debug.Log($"[SevenDayLoginScene] Claiming reward for Day {dayNumber}");
        claimedDays.Add(dayNumber);

        var save = saveSystem ?? SaveSystem.Instance;
        if (save != null && save.Player?.SevenDayLogin != null)
        {
            if (!save.Player.SevenDayLogin.ClaimedDays.Contains(dayNumber))
            {
                save.Player.SevenDayLogin.ClaimedDays.Add(dayNumber);
            }
            save.SaveDataToDisk(GameSaveType.PlayerInfo);
        }

        SevenDayRewardItem reward = GetRewardConfigForDay(dayNumber, SevenDayRewardState.Claimed);
        GiveRewardToPlayer(reward);

        RefreshSevenDayUI();
    }

    private void OnClaimDay7Character(int dayNumber, string characterId)
    {
        Debug.Log($"[SevenDayLoginScene] Claiming Day 7 Character: {characterId}");
        claimedDays.Add(dayNumber);

        var save = saveSystem ?? SaveSystem.Instance;
        if (save != null && save.Player?.SevenDayLogin != null)
        {
            if (!save.Player.SevenDayLogin.ClaimedDays.Contains(dayNumber))
            {
                save.Player.SevenDayLogin.ClaimedDays.Add(dayNumber);
            }
            save.Player.SevenDayLogin.SelectedDay7CharacterId = characterId;
            save.SaveDataToDisk(GameSaveType.PlayerInfo);
        }

        bool alreadyOwned = CheckIfCharacterOwned(characterId);
        int convertedShards = AddCharacterToPlayer(characterId);

        ShowCharacterGachaResult(characterId, alreadyOwned, convertedShards);

        RefreshSevenDayUI();
    }

    private bool CheckIfCharacterOwned(string characterId)
    {
        var save = saveSystem ?? SaveSystem.Instance;
        return save?.Player?.Roster?.Characters != null && save.Player.Roster.Characters.Exists(c => c.ID == characterId);
    }

    private void ShowCharacterGachaResult(string characterId, bool isConverted, int convertedShards)
    {
        if (uiManager == null) return;

        var charConfig = gameDataBase != null ? gameDataBase.GetCharacterConfig(characterId) : null;
        Rare rare = charConfig != null ? Utility.ConvertCharacterRareToItemRare(charConfig.Rare) : Rare.Legendary;
        string charName = charConfig != null 
            ? LocalizationManager.Instance.GetLocalizedValue(charConfig.Name) 
            : characterId;

        var result = new GachaItemResult
        {
            itemId = characterId,
            itemName = !string.IsNullOrEmpty(charName) ? charName : characterId,
            rarity = rare,
            isCharacter = true,
            isConverted = isConverted,
            convertedShardAmount = convertedShards
        };

        uiManager.ShowGachaRewardResult(result, () => {
            if (uiManager != null)
            {
                uiManager.OpenWindowScene(ScreenIds.SevenDayLoginScene);
            }
        });
    }

    private void ShowWeaponGachaResult(string weaponId)
    {
        if (uiManager == null) return;

        var itemConfig = gameDataBase != null ? gameDataBase.GetItemConfig(weaponId) : null;
        Rare rare = itemConfig != null ? itemConfig.Rarity : Rare.Rare;
        string weaponName = itemConfig != null 
            ? LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name) 
            : weaponId;

        var result = new GachaItemResult
        {
            itemId = weaponId,
            itemName = !string.IsNullOrEmpty(weaponName) ? weaponName : weaponId,
            rarity = rare,
            isCharacter = false,
            isConverted = false,
            convertedShardAmount = 0
        };

        uiManager.ShowGachaRewardResult(result, () => {
            if (uiManager != null)
            {
                uiManager.OpenWindowScene(ScreenIds.SevenDayLoginScene);
            }
        });
    }

    private int AddCharacterToPlayer(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) return 0;

        var save = saveSystem ?? SaveSystem.Instance;
        if (save != null && save.Player?.Roster != null)
        {
            if (save.Player.Roster.Characters == null)
            {
                save.Player.Roster.Characters = new List<CharacterSaveData>();
            }

            var charConfig = gameDataBase != null ? gameDataBase.GetCharacterConfig(characterId) : null;
            int shardAmount = charConfig != null 
                ? Utility.GetDuplicateCharacterShardAmount(charConfig.Rare) 
                : 30;

            bool alreadyOwned = save.Player.Roster.Characters.Exists(c => c.ID == characterId);
            if (!alreadyOwned)
            {
                var newChar = new CharacterSaveData
                {
                    ID = characterId,
                    Level = 1,
                    Exp = 0,
                    AscensionTier = 0,
                    StarUp = 0,
                    Weapon = "",
                    Armors = new List<PartSaveData>()
                };
                save.Player.Roster.Characters.Add(newChar);
                UIEvent.OnCharacterAdded?.Invoke(characterId);
                save.SaveDataToDisk(GameSaveType.PlayerInfo);
                return 0;
            }
            else
            {
                // Nếu đã sở hữu -> cộng Shard của vị tướng đó theo độ hiếm
                if (inventoryManager != null)
                {
                    inventoryManager.AddStackableItem(characterId, ItemType.Shard, shardAmount);
                }
                save.SaveDataToDisk(GameSaveType.PlayerInfo);
                return shardAmount;
            }
        }
        return 0;
    }

    private void GiveRewardToPlayer(SevenDayRewardItem reward)
    {
        if (reward == null) return;

        if (reward.Type == SevenDayRewardType.Character)
        {
            bool alreadyOwned = CheckIfCharacterOwned(reward.RewardId);
            int convertedShards = AddCharacterToPlayer(reward.RewardId);
            ShowCharacterGachaResult(reward.RewardId, alreadyOwned, convertedShards);
        }
        else if (reward.Type == SevenDayRewardType.Weapon)
        {
            if (inventoryManager != null)
            {
                for (int i = 0; i < reward.Amount; i++)
                {
                    inventoryManager.AddWeapon(new WeaponSaveData
                    {
                        UUID = System.Guid.NewGuid().ToString(),
                        TemplateID = reward.RewardId,
                        CurrentLevel = 1
                    });
                }
            }
            ShowWeaponGachaResult(reward.RewardId);
        }
        else
        {
            if (reward.Type == SevenDayRewardType.Currency && currencyManager != null)
            {
                if (System.Enum.TryParse<CurrencyType>(reward.RewardId, true, out var cType))
                {
                    currencyManager.Add(cType, reward.Amount);
                }
            }
            else if (inventoryManager != null)
            {
                inventoryManager.AddStackableItem(reward.RewardId, ItemType.Material, reward.Amount);
            }

            if (uiManager != null)
            {
                var popupRewards = new List<RewardItemData> { new RewardItemData(reward.RewardId, reward.Amount) };
                uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(popupRewards));
            }
        }
    }

    /// <summary>
    /// Handles closing the Seven Day Login screen.
    /// </summary>
    public void OnClose()
    {
        if (uiManager != null)
        {
            uiManager.CloseWindowScene(ScreenIds.SevenDayLoginScene);
        }
        else
        {
            UI_Close();
        }
    }

    public override void UI_Close()
    {
        base.UI_Close();
    }

    /// <summary>
    /// Kiểm tra xem người chơi có phần thưởng điểm danh nào ở trạng thái có thể nhận (CanClaim) nhưng chưa nhận hay không.
    /// </summary>
    public static bool HasUnclaimedRewards(SaveSystem saveSys = null)
    {
        var save = saveSys ?? SaveSystem.Instance;
        if (save?.Player == null) return false;

        var data = save.Player.SevenDayLogin;
        if (data == null) return true; // Lần đầu vào game chưa có dữ liệu -> Có quà Ngày 1

        string todayStr = System.DateTime.Now.ToString("yyyy-MM-dd");
        int currentDay = data.CurrentLoginDay <= 0 ? 1 : data.CurrentLoginDay;

        // Nếu qua ngày mới thì currentLoginDay sẽ tăng lên (tối đa 7)
        if (string.IsNullOrEmpty(data.LastLoginDate))
        {
            currentDay = 1;
        }
        else if (data.LastLoginDate != todayStr)
        {
            currentDay = Mathf.Clamp(data.CurrentLoginDay + 1, 1, 7);
        }

        var claimed = data.ClaimedDays ?? new List<int>();

        // Duyệt qua tất cả các ngày từ 1 đến currentDay đã mở khóa
        for (int day = 1; day <= currentDay; day++)
        {
            if (!claimed.Contains(day))
            {
                return true; // Còn ít nhất 1 phần thưởng chưa nhận
            }
        }

        return false; // Tất cả các quà của các ngày đã mở đều đã nhận hết
    }
}
