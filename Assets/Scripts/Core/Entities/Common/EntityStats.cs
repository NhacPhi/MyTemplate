using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EntityStats : StatsController
{
   public EntityStats(Dictionary<StatType, Stat> stats)
    {
        InitStats(stats);
    }
}
