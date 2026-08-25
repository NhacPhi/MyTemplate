using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProfileModel : IStatProvider
{
    public string EntityID { get; private set; }
    public CharacterConfig BaseConfig { get; private set; } // Implement từ IStatProvider
    private int _level;
    public string WeaponID { get; private set; }
    public int WeaponLevel { get; private set; }

    private ItemConfig _weaponConfig;
    public CharacterPassiveManager PassivesManager { get; private set; } = new CharacterPassiveManager();

    public EnemyProfileModel(CharacterConfig config, int level, string entityID = "", string weaponID = "", int weaponLevel = 1, GameDataBase gameDataBase = null)
    {
        BaseConfig = config;
        _level = level;
        EntityID = entityID;
        WeaponID = weaponID;
        WeaponLevel = Mathf.Max(1, weaponLevel);

        PassivesManager.Init(this);

        // 1. Nạp Skill Passives của Enemy / Boss
        if (BaseConfig != null && BaseConfig.Skills != null && gameDataBase != null)
        {
            foreach (var kvp in BaseConfig.Skills)
            {
                if (!string.IsNullOrEmpty(kvp.Value.PassiveID))
                {
                    PassiveConfig passiveCfg = gameDataBase.GetPassiveConfig(kvp.Value.PassiveID);
                    if (passiveCfg != null)
                    {
                        PassivesManager.AddPassive(passiveCfg, 1);
                    }
                }
            }
        }

        // 2. Nạp Vũ Khí & Weapon Passive cho Boss
        if (!string.IsNullOrEmpty(WeaponID) && gameDataBase != null)
        {
            _weaponConfig = gameDataBase.GetItemConfig(WeaponID);
            if (_weaponConfig != null && _weaponConfig.Weapon != null)
            {
                string passiveID = _weaponConfig.Weapon.PassiveID;
                if (!string.IsNullOrEmpty(passiveID))
                {
                    PassiveConfig weaponPassive = gameDataBase.GetPassiveConfig(passiveID);
                    if (weaponPassive != null)
                    {
                        PassivesManager.AddPassive(weaponPassive, WeaponLevel);
                        Debug.Log($"[EnemyProfileModel] Đã trang bị vũ khí '{WeaponID}' (Lv {WeaponLevel}) với nội tại '{passiveID}' cho Boss/Enemy '{EntityID}'!");
                    }
                }
            }
        }
    }

    public int GetTotalStat(StatType type)
    {
        float baseValue = BaseConfig != null ? BaseConfig.GetStat(type) : 0f;

        // Chỉ số cơ bản từ trang bị vũ khí (Flat Stat)
        float weaponFlat = 0f;
        if (_weaponConfig != null && _weaponConfig.Weapon != null && _weaponConfig.Weapon.Stats != null && _weaponConfig.Weapon.Stats.ContainsKey(type))
        {
            weaponFlat = _weaponConfig.Weapon.GetStatByLevel(type, WeaponLevel);
        }

        float pFlat = PassivesManager != null ? PassivesManager.GetTotalFlatBonus(type) : 0f;
        float pPercent = PassivesManager != null ? PassivesManager.GetTotalPercentBonus(type) : 0f;

        if (type == StatType.SPEED || type == StatType.CRIT_RATE || type == StatType.CRIT_DMG || 
            type == StatType.PENETRATION || type == StatType.CRIT_DMG_RES || type == StatType.DEF_SHRED || 
            type == StatType.EHR || type == StatType.RES)
        {
            float total = (baseValue + weaponFlat) * (1f + pPercent / 100f) + pFlat;
            return Mathf.RoundToInt(total);
        }

        float growth = baseValue * 0.1f * (_level - 1);
        float totalBase = baseValue + growth + weaponFlat;
        return Mathf.RoundToInt(totalBase * (1f + pPercent / 100f) + pFlat);
    }

    public float GetBaseStat(StatType type)
    {
        return BaseConfig != null ? BaseConfig.GetStat(type) : 0f;
    }
}
