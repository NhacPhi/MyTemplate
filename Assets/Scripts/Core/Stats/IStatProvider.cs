using UnityEngine;

public interface IStatProvider 
{
    float GetTotalStat(StatType type);
    float GetBaseStat(StatType type);
}
