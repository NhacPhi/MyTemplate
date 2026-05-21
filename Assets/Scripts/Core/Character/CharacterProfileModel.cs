using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using System;
using System.Linq;

/// <summary>
/// Trách nhiệm duy nhất của class này:
///   1. Giữ DATA của nhân vật (SaveData, BaseConfig).
///   2. Quản lý EQUIPMENT (trang bị / tháo trang bị).
///   3. Tính toán STAT tổng hợp (IStatProvider).
///
/// Mọi nghiệp vụ nâng cấp (EXP, Level-Up, Ascend, StarUp)
/// đã được chuyển sang <see cref="CharacterUpgradeManager"/>.
/// </summary>
public class CharacterProfileModel : IStatProvider
{
    private CharacterSaveData _saveData;
    public CharacterSaveData SaveData => _saveData;

    private CharacterConfig _baseConfig;
    public CharacterConfig BaseConfig => _baseConfig;

    private GameDataBase _gameDataBase;
    private InventoryManager _inventory;

    public EquipmentManager Equipment { get; private set; } = new EquipmentManager();
    public CharacterPassiveManager PassivesManager { get; private set; } = new CharacterPassiveManager();

    public event Action OnLevelChanged;
    public event Action OnEquipmentChanged;
    public event Action OnSkilUpgraded;
    public event Action OnStatsChanged;

    private SetBonusEvaluator _setBonusEvaluator;

    // ─────────────────────────────────────────────────────────────
    // KHỞI TẠO
    // ─────────────────────────────────────────────────────────────

    public void Init(
        CharacterSaveData saveData,
        CharacterConfig baseConfig,
        GameDataBase gameDataBase,
        InventoryManager inventory,
        CurrencyManager currency)
    {
        _saveData     = saveData;
        _baseConfig   = baseConfig;
        _gameDataBase = gameDataBase;
        _inventory    = inventory;

        _setBonusEvaluator = new SetBonusEvaluator(_gameDataBase);
        Equipment.Init(_setBonusEvaluator);
        PassivesManager.Init(this);

        LoadEquipmentsFromSave();
        LoadSkillPassives();

        UIEvent.OnEquipmentUpgraded += RefreshEquippedItem;
    }

    public void Dispose()
    {
        UIEvent.OnEquipmentUpgraded -= RefreshEquippedItem;
    }

    private void LoadEquipmentsFromSave()
    {
        // Trang bị vũ khí đã lưu
        if (!string.IsNullOrEmpty(SaveData.Weapon))
        {
            var weaponSave = _inventory.GetWeapon(SaveData.Weapon);
            if (weaponSave != null)
            {
                var weaponConfig = _gameDataBase.GetItemConfig(weaponSave.TemplateID);

                string passiveID = weaponConfig.Weapon.PassiveID;

                PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(passiveID);

                var weaponData   = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig, passiveCfg);
                Equipment.Equip(weaponData);
                PassivesManager.AddPassive(passiveCfg, weaponSave.CurrentUpgrade);
            }
        }

        // Trang bị giáp đã lưu
        if (SaveData.Armors != null)
        {
            foreach (var armorPart in SaveData.Armors)
            {
                var armorSave = _inventory.GetArmor(armorPart.ID);
                if (armorSave != null)
                {
                    var armorConfig = _gameDataBase.GetItemConfig(armorSave.TemplateID);

                    var armorData   = EquipmentFactory.CreateArmorData(armorSave, armorConfig);

                    Equipment.Equip(armorData);
                }
            }
        }
    }

    private void LoadSkillPassives()
    {
        if (_baseConfig.Skills == null) return;

        foreach (var kvp in _baseConfig.Skills)
        {
            SkillCharacter skillType = kvp.Key;
            SkillComponent skillComp = kvp.Value;

            if (!string.IsNullOrEmpty(skillComp.PassiveID))
            {
                PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(skillComp.PassiveID);
                
                // Mức độ cường hóa kỹ năng (0, 1, 2)
                int skillEnhanceLvl = Utility.GetSkillEnhancementLevel(skillType, SaveData.StarUp);
                
                // Do hàm PassiveInstance tính index = Max(0, level - 1), 
                // ta cộng thêm 1 để level bắt đầu từ 1 (khớp với index 0 của mảng giá trị)
                PassivesManager.AddPassive(passiveCfg, skillEnhanceLvl + 1);
            }
        }
    }

    public void RefreshSkillPassives()
    {
        if (_baseConfig.Skills == null) return;

        foreach (var kvp in _baseConfig.Skills)
        {
            if (!string.IsNullOrEmpty(kvp.Value.PassiveID))
            {
                PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(kvp.Value.PassiveID);
                
                // Gỡ nội tại cũ
                PassivesManager.RemovePassive(passiveCfg);

                // Lắp lại nội tại với level mới (nếu StarUp có thay đổi)
                int skillEnhanceLvl = Utility.GetSkillEnhancementLevel(kvp.Key, SaveData.StarUp);
                PassivesManager.AddPassive(passiveCfg, skillEnhanceLvl + 1);
            }
        }
        OnStatsChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────
    // EQUIPMENT – WEAPON
    // ─────────────────────────────────────────────────────────────

    public bool EquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);
        if (weaponSave == null) return false;

        string previousOwnerID = weaponSave.Equip;
        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapWeapon?.Invoke(previousOwnerID, "");
        }

        var weaponConfig   = _gameDataBase.GetItemConfig(weaponSave.TemplateID);

        string passiveID = weaponConfig.Weapon.PassiveID;
        PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(passiveID);

        var runtimeWeapon  = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig, passiveCfg);

        Equipment.Equip(runtimeWeapon);
        PassivesManager.AddPassive(passiveCfg, weaponSave.CurrentUpgrade);
        SaveData.Weapon  = itemUUID;
        weaponSave.Equip = SaveData.ID;

        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
        return true;
    }

    public void UnEquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);
        if (string.IsNullOrEmpty(SaveData.Weapon)) return;

        var weaponConfig = _gameDataBase.GetItemConfig(weaponSave.TemplateID);
        PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(weaponConfig.Weapon.PassiveID);

        Equipment.Unequip(EquipSlot.Weapon);
        PassivesManager.RemovePassive(passiveCfg);
        SaveData.Weapon  = "";
        weaponSave.Equip = "";

        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public bool ChangeWeapon(string newWeaponUUID)
    {
        if (SaveData.Weapon == newWeaponUUID) return false;

        var newWeaponSave = _inventory.GetWeapon(newWeaponUUID);
        if (newWeaponSave == null) return false;

        string myOldWeaponUUID = SaveData.Weapon;
        string previousOwnerID = newWeaponSave.Equip;

        UnEquipWeapon(myOldWeaponUUID);

        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapWeapon?.Invoke(previousOwnerID, myOldWeaponUUID);
        }

        return EquipWeapon(newWeaponUUID);
    }

    // ─────────────────────────────────────────────────────────────
    // EQUIPMENT – ARMOR
    // ─────────────────────────────────────────────────────────────

    public bool EquipArmor(string itemUUID)
    {
        var armorSave = _inventory.GetArmor(itemUUID);
        if (armorSave == null) return false;

        string previousOwnerID = armorSave.Equip;
        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapArmor?.Invoke(previousOwnerID, itemUUID, "");
        }

        var armorConfig  = _gameDataBase.GetItemConfig(armorSave.TemplateID);
        var runtimeArmor = EquipmentFactory.CreateArmorData(armorSave, armorConfig);

        Equipment.Equip(runtimeArmor);

        PartSaveData part = new PartSaveData
        {
            ID   = itemUUID,
            Type = armorConfig.Armor.Part,
        };

        if (!SaveData.Armors.Contains(part))
        {
            SaveData.Armors.Add(part);
        }

        armorSave.Equip = SaveData.ID;

        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool UnequipArmor(string itemUUID)
    {
        PartSaveData equippedPart = SaveData.Armors.Find(p => p.ID == itemUUID);
        if (equippedPart == null) return false;

        var armorSave = _inventory.GetArmor(itemUUID);
        if (armorSave != null)
        {
            armorSave.Equip = "";
        }

        var slotType = EquipmentFactory.ConvertPartToSlot(equippedPart.Type);
        Equipment.Unequip(slotType);
        SaveData.Armors.Remove(equippedPart);

        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool ChangeArmor(string newArmorUUID)
    {
        var newArmorSave = _inventory.GetArmor(newArmorUUID);
        if (newArmorSave == null) return false;

        if (SaveData.Armors.Exists(p => p.ID == newArmorUUID)) return true;

        var armorConfig  = _gameDataBase.GetItemConfig(newArmorSave.TemplateID);
        var targetPart   = armorConfig.Armor.Part;

        PartSaveData myOldPart     = SaveData.Armors.Find(p => p.Type == targetPart);
        string       myOldArmorUUID = myOldPart != null ? myOldPart.ID : "";

        string previousOwnerID = newArmorSave.Equip;

        if (!string.IsNullOrEmpty(myOldArmorUUID))
        {
            UnequipArmor(myOldArmorUUID);
        }

        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapArmor?.Invoke(previousOwnerID, newArmorUUID, myOldArmorUUID);
        }

        return EquipArmor(newArmorUUID);
    }

    public void UnequipAllArmors()
    {
        if (SaveData.Armors == null || SaveData.Armors.Count == 0) return;

        bool hasChanges       = false;
        var  equippedArmors   = new List<PartSaveData>(SaveData.Armors);

        foreach (var part in equippedArmors)
        {
            var armorSave = _inventory.GetArmor(part.ID);
            if (armorSave != null)
            {
                armorSave.Equip = "";
            }

            var slotType = EquipmentFactory.ConvertPartToSlot(part.Type);
            Equipment.Unequip(slotType);
            hasChanges = true;
        }

        SaveData.Armors.Clear();

        if (hasChanges)
        {
            OnEquipmentChanged?.Invoke();
            OnStatsChanged?.Invoke();
        }
    }

    public void QuickEquipArmor()
    {
        List<ArmorSaveData> bestAvailableArmors = _inventory.GetBestArmorsToEquip();
        if (bestAvailableArmors == null || bestAvailableArmors.Count == 0) return;

        foreach (var newArmor in bestAvailableArmors)
        {
            var newConfig  = _gameDataBase.GetItemConfig(newArmor.TemplateID);
            var targetPart = newConfig.Armor.Part;

            PartSaveData currentPart = SaveData.Armors.Find(p => p.Type == targetPart);

            if (currentPart != null)
            {
                var currentArmor = _inventory.GetArmor(currentPart.ID);
                if (currentArmor != null)
                {
                    var currentConfig = _gameDataBase.GetItemConfig(currentArmor.TemplateID);

                    int rarityComparison = newConfig.Rarity.CompareTo(currentConfig.Rarity);
                    int levelComparison  = newArmor.Level.CompareTo(currentArmor.Level);

                    if (rarityComparison < 0 || (rarityComparison == 0 && levelComparison <= 0))
                        continue;
                }
            }

            ChangeArmor(newArmor.UUID);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // SKILL UPGRADE (stub)
    // ─────────────────────────────────────────────────────────────

    public bool UpgradeSkill(string skillID)
    {
        return false;
    }

    // ─────────────────────────────────────────────────────────────
    // STAT CALCULATOR (IStatProvider)
    // ─────────────────────────────────────────────────────────────

    public float GetBaseStat(StatType type)
    {
        float stat = 0f;

        if (_baseConfig.Stats != null)
        {
            stat = _baseConfig.GetStat(type);
        }

        float growth = Utility.GetStatGrowthLevel(SaveData.Level, _baseConfig.GetUpdateStat(type));

        return stat + growth;
    }

    public int GetTotalStat(StatType type)
    {
        float baseStat     = GetBaseStat(type);
        float flatBonus    = Equipment.GetTotalConstantBonus(type) + PassivesManager.GetTotalConstantBonus(type);
        float percentBonus = (Equipment.GetTotalPercentBonus(type) + PassivesManager.GetTotalPercentBonus(type)) / 100f;

        return Mathf.RoundToInt((baseStat + flatBonus) * (1 + percentBonus));
    }

    public float GetTotalArmorConstantStat(StatType type)
    {
        float total = 0f;

        foreach (var kvp in Equipment.EquippedItems)
        {
            EquipSlot     slot = kvp.Key;
            EquipmentData item = kvp.Value;

            if (slot == EquipSlot.Weapon) continue;

            foreach (var mod in item.Modifiers)
            {
                if (mod.Type == type && mod.ModifierType == ModifyType.Constant)
                {
                    total += mod.TotalValue;
                }
            }
        }
        return total;
    }

    // ─────────────────────────────────────────────────────────────
    // SET BONUS
    // ─────────────────────────────────────────────────────────────

    public List<SetBonusConfig> GetSetBonusActive()
    {
        return Equipment.GetActiveSetBonuses();
    }

    public void RefreshEquippedItem(string itemUUID)
    {
        if (SaveData.Weapon == itemUUID)
        {
            var weaponSave = _inventory.GetWeapon(itemUUID);
            if (weaponSave != null)
            {
                var weaponConfig = _gameDataBase.GetItemConfig(weaponSave.TemplateID);

                string passiveID = weaponConfig.Weapon.PassiveID;
                PassiveConfig passiveCfg = _gameDataBase.GetPassiveConfig(passiveID);

                var runtimeWeapon = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig, passiveCfg);

                Equipment.Unequip(EquipSlot.Weapon);
                PassivesManager.RemovePassive(passiveCfg);

                Equipment.Equip(runtimeWeapon);
                PassivesManager.AddPassive(passiveCfg, weaponSave.CurrentUpgrade);

                OnEquipmentChanged?.Invoke();
                OnStatsChanged?.Invoke();
            }
        }

        else if (SaveData.Armors.Exists(p => p.ID == itemUUID))
        {
            var armorSave = _inventory.GetArmor(itemUUID);
            if (armorSave != null)
            {
                var armorConfig = _gameDataBase.GetItemConfig(armorSave.TemplateID);
                var slotType = EquipmentFactory.ConvertPartToSlot(armorConfig.Armor.Part);

                Equipment.Unequip(slotType);

                var runtimeArmor = EquipmentFactory.CreateArmorData(armorSave, armorConfig);
                Equipment.Equip(runtimeArmor);

                OnEquipmentChanged?.Invoke();
                OnStatsChanged?.Invoke();
            }
        }
    }
}
