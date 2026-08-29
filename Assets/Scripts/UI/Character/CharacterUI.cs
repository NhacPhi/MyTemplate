using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject prefabAvatar;

    [SerializeField] private GameObject contentAvatar;

    [SerializeField] private ScrollRect scrollRectl;

    [SerializeField] private List<GameObject> cards;

    List<GameObject> avatars = new();

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private AudioManager audioManager;
    [Inject] private PlayerCharacterManager playerCharacterManager;

    [SerializeField] private Image characterImage;
    [SerializeField] private CharacterWeaponUI waeponUI;
    [SerializeField] private TextMeshProUGUI txtPower;

    [SerializeField] private GameObject characterStatInfo;
    [SerializeField] private GameObject characterWeapon;
    [SerializeField] private GameObject characterArmor;

    [SerializeField] private List<CharacterToggle> taps;

    [SerializeField] private ArmorTooltipUI currentArmorUI;
    [SerializeField] private GameObject bgDetectTrigger;

    [SerializeField] private GameObject lockedImage;

    private string currentCharacter = "";
    private CharacterTap currentTap = CharacterTap.None;

    private bool isSelectedRelicTap = false;
    private bool isOpenWeaponCategpry = false;

    public bool IsCurrentCharacterUnlocked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return false;
        return save != null && save.Player != null && save.Player.Roster != null && save.Player.Roster.GetCharacter(currentCharacter) != null;
    }

    private void OnEnable()
    {
        UIEvent.OnSelectCharacterAvatar += SelectCharacterAvatar;
        UIEvent.OnSelectToggleCharacterTap += ShowCharacterCard;
        UIEvent.OnSelectCharacterChangeWeapon += ShowCharacterWeapon;
        UIEvent.OnCloseCharacterWeapon += CloseCharacterWeapon;
        UIEvent.OnSlectectRelicTap += OnSelectedRelicTap;

        UIEvent.OnShowCharacterCategoryArmor += ShowCharacterArmor;
        UIEvent.OnCloseCharacterCategoryArmor += HideCharacterArmor;
        UIEvent.OnClickArmorCategoryUI += ShowArmorStatTooltipUI;
        UIEvent.OnShowTooltipUI += ShowBackgroundDetectTrigger;

        if (avatars.Count > 0 )
        {
            ResetUI();
            var firstAvatar = avatars[0].GetComponent<CharacterAvatar>();
            if (firstAvatar != null)
            {
                SelectCharacterAvatar(firstAvatar.ID);
            }
            ShowCharacterCard(CharacterTap.Info);
        }
        if (scrollRectl != null)
        {
            scrollRectl.normalizedPosition = new Vector2(0, 1);
        }
    }

    private void OnDisable()
    {
        UIEvent.OnSelectCharacterAvatar -= SelectCharacterAvatar;
        UIEvent.OnSelectToggleCharacterTap -= ShowCharacterCard;

        UIEvent.OnSelectCharacterChangeWeapon -= ShowCharacterWeapon;
        UIEvent.OnCloseCharacterWeapon -= CloseCharacterWeapon;
        UIEvent.OnSlectectRelicTap -= OnSelectedRelicTap;

        UIEvent.OnShowCharacterCategoryArmor -= ShowCharacterArmor;
        UIEvent.OnCloseCharacterCategoryArmor -= HideCharacterArmor;
        UIEvent.OnClickArmorCategoryUI -= ShowArmorStatTooltipUI;
        UIEvent.OnShowTooltipUI -= ShowBackgroundDetectTrigger;
    }

    public void Init()
    {
        foreach (var obj in avatars)
        {
            if (obj != null) Destroy(obj);
        }
        avatars.Clear();

        // 1. Phân loại nhân vật ĐÃ SỞ HỮU (Owned) và CHƯA SỞ HỮU (Locked)
        var ownedSaveList = save.Player.Roster.Characters != null ? save.Player.Roster.Characters : new List<CharacterSaveData>();
        var ownedIDs = new HashSet<string>(ownedSaveList.Select(c => c.ID));

        // Sắp xếp danh sách ĐÃ SỞ HỮU theo LỰC CHIẾN giảm dần
        var sortedOwned = new List<CharacterSaveData>(ownedSaveList);
        sortedOwned.Sort((a, b) =>
        {
            int powerA = playerCharacterManager != null ? playerCharacterManager.GetCharacterPower(a.ID) : 0;
            int powerB = playerCharacterManager != null ? playerCharacterManager.GetCharacterPower(b.ID) : 0;
            int powerComparison = powerB.CompareTo(powerA);
            if (powerComparison != 0) return powerComparison;

            var configA = gameDataBase.GetCharacterConfig(a.ID);
            var configB = gameDataBase.GetCharacterConfig(b.ID);
            if (configA == null || configB == null) return 0;

            int rarityComparison = configB.Rare.CompareTo(configA.Rare);
            if (rarityComparison != 0) return rarityComparison;

            return b.Level.CompareTo(a.Level);
        });

        // Lấy danh sách CHƯA SỞ HỮU từ GameDataBase (chỉ lấy nhân vật có Class == "Character")
        var allCharConfigs = gameDataBase.GetAllCharacterConfigs();
        var lockedCharList = new List<KeyValuePair<string, CharacterConfig>>();
        if (allCharConfigs != null)
        {
            foreach (var kvp in allCharConfigs)
            {
                if (!ownedIDs.Contains(kvp.Key) && kvp.Value != null && kvp.Value.IsPlayableCharacter)
                {
                    lockedCharList.Add(kvp);
                }
            }
        }

        // Sắp xếp danh sách CHƯA SỞ HỮU theo LỰC CHIẾN CƠ BẢN giảm dần
        lockedCharList.Sort((a, b) =>
        {
            int powerA = playerCharacterManager != null ? playerCharacterManager.GetCharacterPower(a.Key) : 0;
            int powerB = playerCharacterManager != null ? playerCharacterManager.GetCharacterPower(b.Key) : 0;
            int powerComparison = powerB.CompareTo(powerA);
            if (powerComparison != 0) return powerComparison;

            int rarityComparison = b.Value.Rare.CompareTo(a.Value.Rare);
            if (rarityComparison != 0) return rarityComparison;

            return string.Compare(a.Key, b.Key, System.StringComparison.Ordinal);
        });

        // 2. Tạo UI Avatar cho nhóm ĐÃ SỞ HỮU trước
        foreach (var character in sortedOwned)
        {
            GameObject obj = Instantiate(prefabAvatar, contentAvatar.transform);
            var charSave = save.Player.Roster.GetCharacter(character.ID);
            string weaponID = charSave != null ? charSave.Weapon : "";
            CharacterConfig config = gameDataBase.GetCharacterConfig(character.ID);
            Sprite icon = config != null ? config.Icon : null;
            obj.GetComponent<CharacterAvatar>().Init(character.ID, weaponID, icon, audioManager, true);
            avatars.Add(obj);
        }

        // 3. Tạo UI Avatar cho nhóm CHƯA SỞ HỮU (nằm ở cuối cùng)
        foreach (var pair in lockedCharList)
        {
            GameObject obj = Instantiate(prefabAvatar, contentAvatar.transform);
            obj.GetComponent<CharacterAvatar>().Init(pair.Key, "", pair.Value.Icon, audioManager, false);
            avatars.Add(obj);
        }

        currencyMM.UpdateCurrency();
        ResetUI();
    }

    private void ResetUI()
    {
        if (avatars.Count == 0) return;

        var firstAvatar = avatars[0].GetComponent<CharacterAvatar>();
        currentCharacter = firstAvatar.ID;
        firstAvatar.SwitchStatus(true);
        ClickOnFristIconAvatar();
        var charConfig = gameDataBase.GetCharacterConfig(currentCharacter);
        if (charConfig != null)
        {
            characterImage.sprite = charConfig.BigIcon;
        }
        currentTap = CharacterTap.None;
        UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
    }

    void ClickOnFristIconAvatar()
    {
        if (avatars.Count > 0 && avatars[0] != null)
        {
            avatars[0].gameObject.GetComponent<CharacterAvatar>().HandleOnClickEvent();
        }
        CloseCharacterWeapon(true);
        HideCharacterArmor();
    }

    public void SelectCharacterAvatar(string id)
    {
        currentCharacter = id;
        var charConfig = gameDataBase.GetCharacterConfig(id);
        if (charConfig != null)
        {
            characterImage.sprite = charConfig.BigIcon;
        }

        bool isUnlocked = IsCurrentCharacterUnlocked();

        if (waeponUI != null)
        {
            waeponUI.gameObject.SetActive(isUnlocked);
        }

        if (isUnlocked)
        {
            var charSave = save.Player.Roster.GetCharacter(currentCharacter);
            string weaponID = charSave != null ? charSave.Weapon : "";
            string weaponTemplateID = "";

            if (!string.IsNullOrEmpty(weaponID))
            {
                var weapData = save.Player.Inventory.GetWeapon(weaponID);
                if (weapData != null) weaponTemplateID = weapData.TemplateID;
            }

            if (!string.IsNullOrEmpty(weaponTemplateID))
            {
                var itemConfig = gameDataBase.GetItemConfig(weaponTemplateID);
                if (itemConfig != null && itemConfig.Weapon != null)
                {
                    waeponUI.SetWeaponImage(itemConfig.Weapon.BigIcon);
                }
                else
                {
                    waeponUI.SetWeaponEmpty();
                }
            }
            else
            {
                waeponUI.SetWeaponEmpty();
            }
        }

        foreach(var obj in avatars)
        {
            if (obj == null) continue;
            CharacterAvatar avatar = obj.GetComponent<CharacterAvatar>();
            if(avatar != null)
            {
                avatar.SwitchStatus(avatar.ID == id);
            }
        }

        txtPower.text = playerCharacterManager.GetCharacterPower(id).ToString();

        foreach (var obj in avatars)
        {
            if (obj == null) continue;
            CharacterAvatar avatar = obj.GetComponent<CharacterAvatar>();
            if (avatar != null)
            {
                avatar.IsShowWeaponCategory = isOpenWeaponCategpry;
            }
        }

        UpdateCardVisibility();
    }

    public void ShowCharacterCard(CharacterTap type)
    {
        currentTap = type;

        foreach(var tap in taps)
        {
            if (tap != null && tap.Type == type)
            {
                tap.ActiveToggle(true);
            }
        }

        UpdateCardVisibility();
    }

    private void UpdateCardVisibility()
    {
        bool isUnlocked = IsCurrentCharacterUnlocked();

        if (!isUnlocked && currentTap != CharacterTap.Info && currentTap != CharacterTap.None)
        {
            // Tướng chưa sở hữu và người chơi đang ở các tab khác Info:
            // Ẩn các card nâng cấp/trang bị, active image thông báo khóa
            foreach (var obj in cards)
            {
                if (obj != null) obj.SetActive(false);
            }

            CloseCharacterWeapon(true);
            HideCharacterArmor();

            if (lockedImage != null)
            {
                lockedImage.SetActive(true);
            }
            return;
        }

        // Tướng đã sở hữu HOẶC đang xem tab Info:
        if (lockedImage != null)
        {
            lockedImage.SetActive(false);
        }

        CharacterTap activeTap = currentTap == CharacterTap.None ? CharacterTap.Info : currentTap;

        foreach (var obj in cards)
        {
            if (obj == null) continue;
            var card = obj.GetComponent<CharacterCard>();
            if (card != null)
            {
                bool show = card.Type == activeTap;
                card.gameObject.SetActive(show);
            }
        }
    }

    public void ShowCharacterWeapon(string id)
    {
        if(isSelectedRelicTap)
        {
            characterStatInfo.gameObject.SetActive(false);
            characterWeapon.gameObject.SetActive(true);
            isOpenWeaponCategpry = true;
        }
    }

    public void CloseCharacterWeapon(bool close)
    {
        if(close)
        {
            characterStatInfo.gameObject.SetActive(true);
            characterWeapon.gameObject.SetActive(false);
            isOpenWeaponCategpry = false;
        }
    }

    public void ShowCharacterArmor(ArmorPart part)
    {
        characterStatInfo.gameObject.SetActive(false);
        characterArmor.gameObject.SetActive(true);
    }

    public void HideCharacterArmor()
    {
        characterStatInfo.gameObject.SetActive(true);
        characterArmor.gameObject.SetActive(false);
    }

    public void OnSelectedRelicTap(bool value)
    {
        isSelectedRelicTap = value;
    }

    public void ShowArmorStatTooltipUI(string id)
    {
        if (currentArmorUI.gameObject.activeSelf && currentArmorUI.CurrentArmorPart == id)
        {
            currentArmorUI.gameObject.SetActive(false);
            bgDetectTrigger.gameObject.SetActive(false);
            return;
        }

        currentArmorUI.gameObject.SetActive(true);
        currentArmorUI.CurrentCharacterID = currentCharacter;
        bgDetectTrigger.gameObject.SetActive(true);
    }

    public void ShowBackgroundDetectTrigger(bool value)
    {
        bgDetectTrigger.gameObject.SetActive(value);
    }
}
