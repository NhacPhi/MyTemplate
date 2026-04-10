using UnityEngine;

public interface IStatProvider 
{
    CharacterConfig BaseConfig { get; }
    int GetTotalStat(StatType type);
    float GetBaseStat(StatType type);
}
