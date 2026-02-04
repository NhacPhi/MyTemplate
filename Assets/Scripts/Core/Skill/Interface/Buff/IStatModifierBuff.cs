using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModifierBuff : IBuff
{
    //Recalculate stats
    void OnCalculateStat(StatType type, ref float value);
}
