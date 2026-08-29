using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VContainer;
using System.Linq;

public class CharacterWeaponCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    [SerializeField] private WeaponCategoryUI prefabsUI;
    [SerializeField] private GameObject content;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventory;

    private List<WeaponCategoryUI> weapons = new();

    private struct WeaponDisplayData
    {
        public string ID;
        public string TemplateID;
        public Rare Rarity;
        public Sprite Icon;
        public Sprite IconBG;
        public Sprite Avatar;
        public int Level;
        public int Upgrade;
        public bool IsUnlocked;
    }

    private void Awake()
    {
        UIEvent.OnSelectCharacterChangeWeapon += ResetWeaponCardCategory;
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += SelectedWeaponCard;
        UIEvent.OnUpdateSingleWeaponCard += UpdateSingleUI;
        UIEvent.OnInventoryChanged += RefreshUI;
        UIEvent.OnWeaponUpgraded += UpdateSingleUI;
        UIEvent.OnEquipmentUpgraded += UpdateSingleUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= SelectedWeaponCard;
        UIEvent.OnUpdateSingleWeaponCard -= UpdateSingleUI;
        UIEvent.OnInventoryChanged -= RefreshUI;
        UIEvent.OnWeaponUpgraded -= UpdateSingleUI;
        UIEvent.OnEquipmentUpgraded -= UpdateSingleUI;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterChangeWeapon -= ResetWeaponCardCategory;
    }

    void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(() =>
            {
                UIEvent.OnCloseCharacterWeapon?.Invoke(true);
                UIEvent.OnSelectToggleCharacterTap?.Invoke(CharacterTap.Relic);
            });
        }
        Init();

        if (inventory.Weapons != null && inventory.Weapons.Count > 0)
        {
            ResetWeaponCardCategory(inventory.Weapons.FirstOrDefault().UUID);
        }
    }

    public void Init()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (gameDataBase == null || inventory == null) return;

        var displayList = new List<WeaponDisplayData>();

        // 1. Danh sách vũ khí ĐÃ SỞ HỮU
        var ownedWeapons = inventory.Weapons != null ? new List<WeaponSaveData>(inventory.Weapons) : new List<WeaponSaveData>();
        
        // Sắp xếp vũ khí sở hữu theo Rarity giảm dần, Level giảm dần, Upgrade giảm dần
        ownedWeapons.Sort((a, b) =>
        {
            var cfgA = gameDataBase.GetItemConfig(a.TemplateID);
            var cfgB = gameDataBase.GetItemConfig(b.TemplateID);
            Rare rareA = cfgA != null ? cfgA.Rarity : Rare.Common;
            Rare rareB = cfgB != null ? cfgB.Rarity : Rare.Common;

            int rareComp = rareB.CompareTo(rareA);
            if (rareComp != 0) return rareComp;

            int levelComp = b.CurrentLevel.CompareTo(a.CurrentLevel);
            if (levelComp != 0) return levelComp;

            return b.CurrentUpgrade.CompareTo(a.CurrentUpgrade);
        });

        foreach (var item in ownedWeapons)
        {
            var weaponConfig = gameDataBase.GetItemConfig(item.TemplateID);
            if (weaponConfig == null) continue;

            Sprite avatar = !string.IsNullOrEmpty(item.Equip) ? gameDataBase.GetCharacterConfig(item.Equip)?.Icon : null;
            displayList.Add(new WeaponDisplayData
            {
                ID = item.UUID,
                TemplateID = item.TemplateID,
                Rarity = weaponConfig.Rarity,
                Icon = weaponConfig.Icon,
                IconBG = weaponConfig.IconBG,
                Avatar = avatar,
                Level = item.CurrentLevel,
                Upgrade = item.CurrentUpgrade,
                IsUnlocked = true
            });
        }

        // 2. Danh sách vũ khí CHƯA SỞ HỮU
        var ownedTemplateIDs = new HashSet<string>(ownedWeapons.Select(w => w.TemplateID));
        var allWeaponConfigs = gameDataBase.GetAllWeaponConfigs();
        var unownedList = new List<KeyValuePair<string, ItemConfig>>();

        if (allWeaponConfigs != null)
        {
            foreach (var kvp in allWeaponConfigs)
            {
                if (!ownedTemplateIDs.Contains(kvp.Key) && kvp.Value != null && kvp.Value.Weapon != null)
                {
                    unownedList.Add(kvp);
                }
            }
        }

        // Sắp xếp vũ khí chưa sở hữu theo Rarity giảm dần
        unownedList.Sort((a, b) =>
        {
            int rareComp = b.Value.Rarity.CompareTo(a.Value.Rarity);
            if (rareComp != 0) return rareComp;
            return string.Compare(a.Key, b.Key, System.StringComparison.Ordinal);
        });

        foreach (var kvp in unownedList)
        {
            displayList.Add(new WeaponDisplayData
            {
                ID = kvp.Key,
                TemplateID = kvp.Key,
                Rarity = kvp.Value.Rarity,
                Icon = kvp.Value.Icon,
                IconBG = kvp.Value.IconBG,
                Avatar = null,
                Level = 1,
                Upgrade = 0,
                IsUnlocked = false
            });
        }

        // 3. Render danh sách lên UI (tái sử dụng pool)
        for (int i = 0; i < displayList.Count; i++)
        {
            var data = displayList[i];
            WeaponCategoryUI weaponUI;

            if (i < weapons.Count)
            {
                weaponUI = weapons[i];
                weaponUI.gameObject.SetActive(true);
            }
            else
            {
                var obj = Instantiate(prefabsUI, content.transform);
                weaponUI = obj.GetComponent<WeaponCategoryUI>();
                weaponUI.gameObject.SetActive(true);
                weapons.Add(weaponUI);
            }

            weaponUI.Init(data.ID, data.Rarity, data.Icon, data.IconBG, data.Avatar, data.Level, data.Upgrade, data.IsUnlocked);
        }

        for (int i = displayList.Count; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void UpdateSingleUI(string uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return;
        var weaponUI = weapons.Find(x => x != null && x.ID == uuid);
        if (weaponUI != null) 
        {
            var itemData = inventory.GetWeapon(uuid);
            if (itemData != null)
            {
                var weaponConfig = gameDataBase.GetItemConfig(itemData.TemplateID);
                Sprite avatar = !string.IsNullOrEmpty(itemData.Equip) ? gameDataBase.GetCharacterConfig(itemData.Equip)?.Icon : null;
                weaponUI.Init(itemData.UUID, weaponConfig.Rarity, weaponConfig.Icon,
                    weaponConfig.IconBG, avatar, itemData.CurrentLevel, itemData.CurrentUpgrade, true);
            }
        }
    }

    public void SelectedWeaponCard(string id)
    {
        ResetWeaponCards();
    }

    private void ResetWeaponCards()
    {
        foreach (var weapon in weapons)
        {
            if (weapon != null) weapon.OnSwitchStatusBoder(false);
        }
    }

    public void ResetWeaponCardCategory(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            foreach (var weapon in weapons)
            {
                if (weapon != null) weapon.OnSwitchStatusBoder(false);
            }
        }
        else
        {
            foreach (var weapon in weapons)
            {
                if (weapon == null) continue;
                weapon.OnSwitchStatusBoder(weapon.ID == id);
            }
        }
    }
}
