using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;

public class SevenDayLoginUI : MonoBehaviour
{
    [Header("Day & Title")]
    [SerializeField] private TextMeshProUGUI txtDay;
    [SerializeField] private TextMeshProUGUI txtItemName;

    [Header("Item Container")]
    [Tooltip("Container (Transform/GameObject) nơi các Prefab GameItemUI, CharacterIconUI, WeaponUI được sinh ra")]
    [SerializeField] private Transform itemContainer;

    [Header("Claim States")]
    [SerializeField] private GameObject claimedOverlay;       // Trạng thái đã nhận (imgOverlay)
    [SerializeField] private UIClaimHighlightEffect claimHighlightEffect; // Hiệu ứng lấp lánh trên root

    [Header("Actions")]
    [SerializeField] private Button btnClaim;                 // Button để nhận reward


    private GameDataBase _gameDataBase;
    private SevenDayRewardItem _rewardData;
    private Action<int> _onClaimClicked;
    private GameObject _currentSpawnedItemGO;

    private void Awake()
    {
        AutoWire();

        if (btnClaim != null)
        {
            btnClaim.onClick.AddListener(OnClickClaim);
        }
    }

    private void OnDestroy()
    {
        if (btnClaim != null)
        {
            btnClaim.onClick.RemoveListener(OnClickClaim);
        }
    }

    public void AutoWire()
    {
        if (txtDay == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t.name.Contains("STR_DAY") || t.name.Contains("txtDay") || t.name.Contains("Day"))
                {
                    txtDay = t;
                    break;
                }
            }
        }

        if (txtItemName == null)
        {
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t != txtDay && (t.name.Contains("Name") || t.name.Contains("txtItemName")))
                {
                    txtItemName = t;
                    break;
                }
            }
        }

        if (itemContainer == null)
        {
            var contentT = transform.Find("Content");
            itemContainer = contentT != null ? contentT : transform;
        }

        if (claimedOverlay == null)
        {
            var overlayT = transform.Find("imgOverlay");
            if (overlayT != null) claimedOverlay = overlayT.gameObject;
        }

        if (claimHighlightEffect == null)
        {
            claimHighlightEffect = GetComponent<UIClaimHighlightEffect>() ?? gameObject.AddComponent<UIClaimHighlightEffect>();
        }

        if (btnClaim == null)
        {
            btnClaim = GetComponentInChildren<Button>(true);
        }
    }

    public void InitDependencies(GameDataBase db)
    {
        if (_gameDataBase == null)
        {
            _gameDataBase = db;
        }
    }

    /// <summary>
    /// Thiết lập phần thưởng và tự động sinh (Instantiate) component UI tương ứng từ Prefab được truyền từ Scene Manager
    /// </summary>
    public void Setup(
        SevenDayRewardItem rewardData, 
        Action<int> onClaimClicked, 
        GameDataBase db = null, 
        GameItemUI itemPrefab = null, 
        CharacterIconUI charPrefab = null, 
        WeaponUI weaponPrefab = null)
    {
        AutoWire();
        _rewardData = rewardData;
        _onClaimClicked = onClaimClicked;
        if (db != null) _gameDataBase = db;

        if (_rewardData == null) return;

        SetDayText(_rewardData.DayNumber);
        InitRewardSubView(_rewardData, itemPrefab, charPrefab, weaponPrefab);
        SetState(_rewardData.State);
    }

    /// <summary>
    /// Tự động lựa chọn và khởi tạo Sub-View từ Prefab dựa vào Data Config
    /// </summary>
    private void InitRewardSubView(
        SevenDayRewardItem reward, 
        GameItemUI itemPrefab, 
        CharacterIconUI charPrefab, 
        WeaponUI weaponPrefab)
    {
        Transform container = itemContainer != null ? itemContainer : transform;

        // Xóa item đã spawn trước đó (nếu có)
        if (_currentSpawnedItemGO != null)
        {
            Destroy(_currentSpawnedItemGO);
            _currentSpawnedItemGO = null;
        }

        // 1. Nếu là Character -> Dùng characterIconPrefab
        if (reward.Type == SevenDayRewardType.Character)
        {
            if (_gameDataBase != null)
            {
                var charConfig = _gameDataBase.GetCharacterConfig(reward.RewardId);
                var itemConfig = _gameDataBase.GetItemConfig(reward.RewardId);

                if (charConfig != null)
                {
                    string cName = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetLocalizedValue(charConfig.Name) 
                        : reward.RewardId;
                    SetItemName(!string.IsNullOrEmpty(cName) ? cName : reward.RewardId);

                    Rare rare = Utility.ConvertCharacterRareToItemRare(charConfig.Rare);
                    Sprite icon = charConfig.Icon;
                    Sprite bg = itemConfig != null ? itemConfig.IconBG : null;

                    if (charPrefab != null)
                    {
                        var charUI = Instantiate(charPrefab, container);
                        _currentSpawnedItemGO = charUI.gameObject;
                        charUI.Init(reward.RewardId, rare, icon, bg, 1, 0, null);
                    }
                    else if (itemPrefab != null)
                    {
                        var itemUI = Instantiate(itemPrefab, container);
                        _currentSpawnedItemGO = itemUI.gameObject;
                        itemUI.Setup(reward.RewardId, rare, icon, bg);
                        itemUI.SetAmount(reward.Amount);
                    }
                    else
                    {
                        // Fallback tìm component có sẵn dưới hierarchy
                        var existingChar = container.GetComponentInChildren<CharacterIconUI>(true);
                        if (existingChar != null)
                        {
                            existingChar.gameObject.SetActive(true);
                            existingChar.Init(reward.RewardId, rare, icon, bg, 1, 0, null);
                        }
                    }
                    return;
                }
            }

            SetItemName(!string.IsNullOrEmpty(reward.CustomName) ? reward.CustomName : reward.RewardId);
            return;
        }

        // 2. Nếu là Weapon -> Dùng weaponPrefab (hoặc fallback itemPrefab)
        if (reward.Type == SevenDayRewardType.Weapon)
        {
            if (_gameDataBase != null)
            {
                var itemConfig = _gameDataBase.GetItemConfig(reward.RewardId);
                if (itemConfig != null)
                {
                    string wName = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name) 
                        : reward.RewardId;
                    SetItemName(!string.IsNullOrEmpty(wName) ? wName : reward.RewardId);

                    if (weaponPrefab != null)
                    {
                        var wUI = Instantiate(weaponPrefab, container);
                        _currentSpawnedItemGO = wUI.gameObject;
                        wUI.Init(reward.RewardId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG, 1, 0);
                    }
                    else if (itemPrefab != null)
                    {
                        var itemUI = Instantiate(itemPrefab, container);
                        _currentSpawnedItemGO = itemUI.gameObject;
                        itemUI.Setup(reward.RewardId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                        itemUI.SetAmount(reward.Amount);
                    }
                    else
                    {
                        var existingWeapon = container.GetComponentInChildren<WeaponUI>(true);
                        if (existingWeapon != null)
                        {
                            existingWeapon.gameObject.SetActive(true);
                            existingWeapon.Init(reward.RewardId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG, 1, 0);
                        }
                    }
                    return;
                }
            }

            SetItemName(!string.IsNullOrEmpty(reward.CustomName) ? reward.CustomName : reward.RewardId);
            return;
        }

        // 3. Mặc định là Item, Material, Armor, Currency -> Dùng itemPrefab (GameItemUI)
        if (_gameDataBase != null)
        {
            var itemConfig = _gameDataBase.GetItemConfig(reward.RewardId);
            if (itemConfig != null)
            {
                string iName = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name) 
                    : reward.RewardId;
                SetItemName(!string.IsNullOrEmpty(iName) ? iName : reward.RewardId);

                if (itemPrefab != null)
                {
                    var itemUI = Instantiate(itemPrefab, container);
                    _currentSpawnedItemGO = itemUI.gameObject;
                    itemUI.Setup(reward.RewardId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                    itemUI.SetAmount(reward.Amount);
                }
                else
                {
                    var existingItem = container.GetComponentInChildren<GameItemUI>(true);
                    if (existingItem != null)
                    {
                        existingItem.gameObject.SetActive(true);
                        existingItem.Setup(reward.RewardId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                        existingItem.SetAmount(reward.Amount);
                    }
                }
                return;
            }
        }

        SetItemName(!string.IsNullOrEmpty(reward.CustomName) ? reward.CustomName : reward.RewardId);
    }

    /// <summary>
    /// Cập nhật hiển thị theo 2 trạng thái: Có thể nhận và Đã nhận
    /// </summary>
    public void SetState(SevenDayRewardState state)
    {
        if (_rewardData != null) _rewardData.State = state;

        switch (state)
        {
            case SevenDayRewardState.CanClaim:
                // Trạng thái 1: Có thể nhận
                if (claimedOverlay != null) claimedOverlay.SetActive(false);
                if (claimHighlightEffect != null) claimHighlightEffect.Play();

                if (btnClaim != null)
                {
                    btnClaim.interactable = true;
                    btnClaim.gameObject.SetActive(true);
                }
                break;

            case SevenDayRewardState.Claimed:
                // Trạng thái 2: Đã nhận rồi
                if (claimedOverlay != null) claimedOverlay.SetActive(true);
                if (claimHighlightEffect != null) claimHighlightEffect.Stop();

                if (btnClaim != null)
                {
                    btnClaim.interactable = false;
                }
                break;

            case SevenDayRewardState.Locked:
            default:
                // Trạng thái chưa tới ngày mở
                if (claimedOverlay != null) claimedOverlay.SetActive(false);
                if (claimHighlightEffect != null) claimHighlightEffect.Stop();

                if (btnClaim != null)
                {
                    btnClaim.interactable = false;
                }
                break;
        }
    }

    public void SetDayText(int dayNumber)
    {
        if (txtDay != null)
        {
            string locKey = $"STR_DAY_0{dayNumber}";
            string dayText = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetLocalizedValue(locKey) 
                : "";
            if (string.IsNullOrEmpty(dayText) || dayText == locKey)
            {
                dayText = $"Ngày {dayNumber}";
            }
            txtDay.text = dayText;
        }
    }

    public void SetItemName(string name)
    {
        if (txtItemName != null)
        {
            txtItemName.text = name;
        }
    }

    private void OnClickClaim()
    {
        if (_rewardData != null && _rewardData.State == SevenDayRewardState.CanClaim)
        {
            _onClaimClicked?.Invoke(_rewardData.DayNumber);
        }
    }

    public Transform ItemContainer => itemContainer;
    public SevenDayRewardItem RewardData => _rewardData;
    public int DayNumber => _rewardData != null ? _rewardData.DayNumber : 0;
}
