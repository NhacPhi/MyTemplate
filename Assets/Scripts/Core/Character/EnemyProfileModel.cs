using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProfileModel : IStatProvider
{
    public string EntityID { get; private set; }
    public CharacterConfig BaseConfig { get; private set; } // Implement từ IStatProvider
    private int _level;


    public EnemyProfileModel(CharacterConfig config, int level, string entityID = "")
    {
        BaseConfig = config;
        _level = level;
        EntityID = entityID;
    }

    public int GetTotalStat(StatType type)
    {
        float baseValue = BaseConfig.GetStat(type);
        if (type == StatType.SPEED || type == StatType.CRIT_RATE || type == StatType.CRIT_DMG || 
            type == StatType.PENETRATION || type == StatType.CRIT_DMG_RES || type == StatType.DEF_SHRED || 
            type == StatType.EHR || type == StatType.RES)
        {
            return Mathf.RoundToInt(baseValue);
        }

        float growth = baseValue * 0.1f * (_level - 1);
        return Mathf.RoundToInt(baseValue + growth);
    }

    public float GetBaseStat(StatType type)
    {
        return BaseConfig.GetStat(type);
    }
}
