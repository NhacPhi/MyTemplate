using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using System;
using System.Linq;

public class CharacterProfileModel : IStatProvider
{
    private CharacterSaveData _saveData;

    public CharacterSaveData SaveData => _saveData;
    private CharacterConfig _baseConfig;

    private GameDataBase _gameDataBaae;
    private InventoryManager _inventory;

    public EquipmentManager Equipment { get; private set; } = new EquipmentManager();

    public event Action OnLevelChanged;
    public event Action OnEquipmentChanged;
    public event Action OnSkilUpgraded;
    public event Action OnStatsChanged;

    public void Init(CharacterSaveData saveData,  CharacterConfig baseConfig, GameDataBase gameDataBase, InventoryManager inventory)
    {
        _saveData = saveData;
        _baseConfig = baseConfig;
        _gameDataBaae = gameDataBase;
        _inventory = inventory;

        LoadEquipmentsFromSave();
    }

    private void LoadEquipmentsFromSave()
    {
        // check weapon equip
        if(!string.IsNullOrEmpty(SaveData.Weapon))
        {
            var weaponSave = _inventory.GetWeapon(SaveData.Weapon);
            if(weaponSave != null)
            { 
                var weaponConfig = _gameDataBaae.GetItemConfig(weaponSave.TemplateID);
                var weaponData = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig);
                Equipment.Equip(weaponData);
            }
        }

        // check armor equip
        if(SaveData.Armors != null)
        {
            foreach(var armorPart in SaveData.Armors)
            {
                var armorSave = _inventory.GetArmor(armorPart.ID);
                if(armorSave != null)
                {
                    var armorConfig = _gameDataBaae.GetItemConfig(armorSave.TemplateID);
                    var armorData = EquipmentFactory.CreateArmorData(armorSave, armorConfig);
                    Equipment.Equip(armorData);
                }
            }
        }
    }

    public void AddExp(int amount)
    {

    }

    private int GetExpRequireForNextLevel(int currentLevel)
    {
        return 0;
    }

    public void EquipItemFromInventory(string itemUUID)
    {

    }

    public bool EquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);

        if(weaponSave == null) return false;

        var weaponConfig = _gameDataBaae.GetItemConfig(weaponSave.TemplateID);
        var runtimeWeapon = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig);

        Equipment.Equip(runtimeWeapon);

        SaveData.Weapon = itemUUID;

        weaponSave.Equip = SaveData.ID;

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
        
        return true;
    }

    public void UnEquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);

        if (string.IsNullOrEmpty(SaveData.Weapon)) return;

        Equipment.Unequip(EquipSlot.Weapon);

        SaveData.Weapon = "";
        weaponSave.Equip = "";

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public bool EquipArmor(string itemUUID)
    {
        var armorSave = _inventory.GetArmor(itemUUID);
        if(armorSave == null) return false;

        var armorConfig = _gameDataBaae.GetItemConfig(armorSave.TemplateID);
        var runtimeArmor = EquipmentFactory.CreateArmorData(armorSave, armorConfig);

        Equipment.Equip(runtimeArmor);

        PartSaveData part = new PartSaveData
        {
            ID = itemUUID,
            Type = armorConfig.Armor.Part,
        };

        if(SaveData.Armors.Contains(part))
        {
            SaveData.Armors.Add(part);
        }

        armorSave.Equip = SaveData.ID;

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();

        return true;
    }

    public bool UpgradeSkill(string skillID)
    {
        return false;
    }

    public float GetBaseStat(StatType type)
    {
        float baseValue = 0f;

        if(_baseConfig.Stats != null)
        {
            baseValue = _baseConfig.GetStat(type);
        }

        float growth = baseValue * 0.1f * (SaveData.Level - 1);

        return baseValue + growth;
    }

    public float GetTotalStat(StatType type)
    {
        float baseStat = GetBaseStat(type);

        float flatBonus = Equipment.GetTotalConstantBonus(type);

        float percentBonus = Equipment.GetTotalPercenBonus(type) / 100;

        float totalStat = (baseStat + flatBonus) * (1 + percentBonus);

        return totalStat;
    }

    public float GetTotalArmorConstantStat(StatType type)
    {
        float total = 0f;

        foreach (var kvp in Equipment.EquippedItems)
        {
            EquipSlot slot = kvp.Key;
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

}
