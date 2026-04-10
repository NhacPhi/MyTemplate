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

    public int GetTotalStat(StatType type)
    {
        float baseValue = BaseConfig.GetStat(type);
        float growth    = baseValue * 0.1f * (_level - 1);

        return Mathf.RoundToInt(baseValue + growth);
    }

    public float GetBaseStat(StatType type)
    {
        return BaseConfig.GetStat(type);
    }
}
