using UnityEngine;

public abstract class SkillData
{
    //private string id;

    public string ID { get; set; }

    public SkillType SkillType { get; set; }

    public SkillTargetType TargetType;

    public float DamageMultiplier;

    public int MaxCoolDown;

    public EffectConfig Effect;

    public string PassiveID { get; set; }

    public string Sound { get; set; }

    public bool IsBuffSkill => SkillType == SkillType.NonAttackSkill || (Effect != null && Effect.IsBuff());

    public abstract SkillRuntime CreateRuntimeSkill(EntityStats owner);
}
