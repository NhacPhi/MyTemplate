using UnityEngine;

public interface IStatProvider 
{
    string EntityID { get; }
    CharacterConfig BaseConfig { get; }
    int GetTotalStat(StatType type);
    float GetBaseStat(StatType type);
}
