using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillDataFactory
{
    public static SkillData Create(Skill type)
    {
        switch (type)
        {
            case Skill.None: return null;

            case Skill.Melee: return new MeleeAttackData();

            case Skill.Range: return new RangeMagicAttackData();

            case Skill.MajorAttack: return new MajorAttackData();

            case Skill.Summon: return new SummonSkillData();

            case Skill.StatModifier: return new StatModifierData();

            case Skill.BuffShield: return new BuffShieldData();

            case Skill.Healing: return new HealingData();

            case Skill.FireRing: return new RingOfUniverseData();

            case Skill.ThunderBall: return new ThunderBallData();

            case Skill.FireBall: return new FireBallData();

            case Skill.EmpowerAttack: return new EmpoweredAttackData();

            case Skill.Torando: return new TorandoData();

            case Skill.Suriken: return new SurikenData();

            case Skill.PoisonBall: return new PoíonBallData();

            case Skill.DivineWind: return new DivineWindData();
        }

        return null;
    }

}
