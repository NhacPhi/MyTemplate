using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProfileModel : IStatProvider
{
    public CharacterConfig BaseConfig { get; private set; } // Implement từ IStatProvider
    private int _level;


    public EnemyProfileModel(CharacterConfig config, int level)
    {
        BaseConfig = config;
        _level = level;
    }

    public float GetBaseStat(StatType type)
    {
        if (BaseConfig == null) return 0f;
        float baseStat = BaseConfig.GetStat(type);
        float updateStat = BaseConfig.GetUpdateStat(type);
        float growth = Utility.GetStatGrowthLevel(_level, updateStat);
        return baseStat + growth;
    }

    public int GetTotalStat(StatType type)
    {
        return Mathf.RoundToInt(GetBaseStat(type));
    }
}
