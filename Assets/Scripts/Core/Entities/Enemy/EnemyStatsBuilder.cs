using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsBuilder : EntityStatsBuilder
{
    public override EntityStats Build()
    {
        return new EnemyStats(stats);
    }
}
