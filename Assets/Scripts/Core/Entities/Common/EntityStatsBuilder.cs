using System.Collections.Generic;
using Unity.VisualScripting;


public abstract class EntityStatsBuilder
{
    protected Dictionary<StatType, Stat> stats = new();
    public EntityStatsBuilder SetBase(Dictionary<StatType, float> baseStats)
    {
        foreach (var kv in baseStats)
        {
            if (!stats.ContainsKey(kv.Key))
                stats[kv.Key] = new Stat(kv.Value);
        }

        return this;
    }

    public abstract EntityStats Build();

}
