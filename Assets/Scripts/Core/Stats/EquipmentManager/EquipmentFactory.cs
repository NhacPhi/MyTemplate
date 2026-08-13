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
                    ModifierType = ModifyType.Flat,
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

    public static EquipmentData CreateArmorData(ArmorSaveData saveData, ItemConfig config, GameDataBase gameData = null)
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

        if (mainStat != null)
        {
            StatType actualMainStatType = (saveData.MainStatType != StatType.None) 
                ? saveData.MainStatType 
                : mainStat.Type;

            runtimeArmor.Modifiers.Add(new EquipModifier()
            {
                Type = actualMainStatType,
                ModifierType = mainStat.ModifierType,
                BaseValue = Utility.GetArmorMainStatByLevel(mainStat.Value, saveData.Level, saveData.Rare),
                UpgradeBonus = 0
            });
        }

        if (saveData.Substats != null)
        {
            SubstatPoolConfig poolConfig = null;
            if (gameData != null && !string.IsNullOrEmpty(config.Armor.SubstatPoolID))
            {
                poolConfig = gameData.GetSubstatPoolConfig(config.Armor.SubstatPoolID);
            }

            foreach (var sub in saveData.Substats)
            {
                float finalSubValue = 0;

                // Tự động Recalculate Substat Value dựa theo Excel Config mới nhất & Level trong Save
                if (poolConfig != null && poolConfig.Pools != null)
                {
                    var poolComp = poolConfig.Pools.Find(p => p.Type == sub.Type && p.ModifierType == sub.ModifierType) 
                                ?? poolConfig.Pools.Find(p => p.Type == sub.Type);
                    if (poolComp != null)
                    {
                        // Giá trị trung bình của 1 lần roll từ Pool Excel nhân với số lần roll (Level)
                        float avgPerRoll = (poolComp.Min + poolComp.Max) * 0.5f;
                        int rollCount = Mathf.Max(1, sub.Level);
                        finalSubValue = Mathf.Round(avgPerRoll * rollCount);
                        sub.SetCalculatedValue((int)finalSubValue);
                    }
                }
                else
                {
                    finalSubValue = sub.Value;
                }

                runtimeArmor.Modifiers.Add(new EquipModifier
                {
                    Type = sub.Type,
                    ModifierType = sub.ModifierType,
                    BaseValue = finalSubValue,
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
