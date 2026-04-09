using UnityEngine;

public interface IStatProvider 
{
    CharacterConfig BaseConfig { get; }
    float GetTotalStat(StatType type);
    float GetBaseStat(StatType type);
}
