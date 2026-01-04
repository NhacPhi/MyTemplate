using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class CharacterStatsBuilder : EntityStatsBuilder
{
    public EntityStatsBuilder ApplyArmorEquipment(List<ArmorSaveData> armors)
    {
        foreach (var armor in armors)
        {
            foreach (var stat in armor.Stats)
            {
                if (!stats.ContainsKey(stat.Type))
                    stats[stat.Type] = new Stat(0);

                stats[stat.Type].AddModifier(new Modifier(stat.Point, stat.ModifierType));
            }

        }
        return this;
    }
    // Apply Bonus Armorset
    // Apply Weapon
    // Apply Level
    // Apply Talen
    /// <summary>
    /// ...
    /// </summary>
    /// <returns></returns>
    public override EntityStats Build()
    {
        return new CharacterStats(stats);
    }
}
