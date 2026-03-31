using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentFactory 
{
    public static EquipmentData CreateWeaponData(WeaponSaveData save, ItemConfig config)
    {
        if (config.Type != ItemType.Weapon || config.Weapon == null) return null;

        var uuid = save.UUID;
        var level = save.CurrentLevel;

        var runtimeWeapon = new EquipmentData()
        {
            UUID = uuid,
            Level = level,
            Slot = EquipSlot.Weapon,
            BaseConfig = config
        };

        if(config.Weapon.Stats != null)
        {
            foreach(var kvp  in config.Weapon.Stats)
            {
                StatType statType = kvp.Key;
                float baseVal = kvp.Value;

                float upgradeVal = 0;

                if(config.Weapon.Upgrades != null && config.Weapon.Upgrades.
                    TryGetValue(statType, out int upgradePerLevel))
                {
                    upgradeVal = upgradePerLevel * level;
                }

                runtimeWeapon.Modifiers.Add(new EquipModifier()
                {
                    Type = statType,
                    ModifierType = ModifyType.Constant,
                    BaseValue = baseVal,
                    UpgradeBonus = upgradeVal
                });
            }

        }

        return runtimeWeapon;
    }

    public static EquipmentData CreateArmorData(ArmorSaveData saveData, ItemConfig config)
    {
        if(config.Type != ItemType.Armor || config.Armor == null) return null;

        var runtimeArmor = new EquipmentData()
        {
            UUID = saveData.UUID,
            Level = saveData.Level,
            Slot = ConvertPartToSlot(config.Armor.Part),
            SetName = config.Armor.ArmorSet,
            BaseConfig = config
        };

        var mainStat = config.Armor.MainStat;

        if(mainStat != null)
        {
            float upgradeBonus = (mainStat.Value * 0.1f) * saveData.Level;

            runtimeArmor.Modifiers.Add(new EquipModifier()
            {
                Type = mainStat.Type,
                ModifierType = mainStat.ModifierType,
                BaseValue = mainStat.Value,
                UpgradeBonus = upgradeBonus
            });
        }

        if(saveData.Substats != null)
        {
            foreach(var sub in saveData.Substats)
            {
                runtimeArmor.Modifiers.Add(new EquipModifier
                {
                    Type = sub.Type,
                    ModifierType = sub.ModifierType,
                    BaseValue = sub.Value,
                    UpgradeBonus = 0
                });
            }
        }

        return runtimeArmor;
    }

    public static EquipSlot ConvertPartToSlot(ArmorPart part)
    {
        return (EquipSlot)Enum.Parse(typeof(EquipSlot), part.ToString());
    }
}
