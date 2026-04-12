using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentFactory 
{
    public static EquipmentData CreateWeaponData(WeaponSaveData save, ItemConfig config, PassiveConfig passiveCfg)
    {
        if (config.Type != ItemType.Weapon || config.Weapon == null) return null;

        var uuid = save.UUID;
        var level = save.CurrentLevel;
        var upgrade = save.CurrentUpgrade;

        var runtimeWeapon = new EquipmentData()
        {
            UUID = uuid,
            Level = level,
            Slot = EquipSlot.Weapon,
            BaseConfig = config
        };

        // base Stats
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

        // Passive (static modifiers)
        if(passiveCfg != null)
        {
            if (passiveCfg != null && passiveCfg.StaticModifiers != null)
            {
                int index = Mathf.Max(0, upgrade - 1); // Upgrade 1 tương ứng index 0

                foreach (var staticMod in passiveCfg.StaticModifiers)
                {
                    // Chuyển string từ JSON sang Enum (Nếu data đã là Enum thì bỏ qua Parse)
                    if (System.Enum.TryParse(staticMod.StatType, out StatType sType) &&
                        System.Enum.TryParse(staticMod.ModifyType, out ModifyType mType))
                    {
                        // Lấy giá trị tương ứng với Level vũ khí
                        float valAtLevel = staticMod.ModifyByUpgrade[Mathf.Min(index, staticMod.ModifyByUpgrade.Count - 1)];

                        runtimeWeapon.Modifiers.Add(new EquipModifier()
                        {
                            Type = sType,
                            ModifierType = mType, // Có thể là Percent hoặc Constant tùy config
                            BaseValue = valAtLevel,
                            UpgradeBonus = 0 // Vì giá trị trong bảng Static đã tính theo Level rồi
                        });
                    }
                }
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
            float upgradeBonus = 0;

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
