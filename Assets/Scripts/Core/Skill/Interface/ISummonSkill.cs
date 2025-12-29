using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface ISummonSkill 
{
    List<Entity> ActiveSummons { get; }
    UniTask PerformSummon(SkillData data, Entity caster);

    void ClearSummons();
}
