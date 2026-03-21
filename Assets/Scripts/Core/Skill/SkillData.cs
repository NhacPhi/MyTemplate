using UnityEngine;

public abstract class SkillData
{
    //private string id;

    public string ID { get; set; }

    public SkillTargetType TargetType;

    public float DamageMultiplier;

    public int MaxCoolDown;

    public float FlatDamage;

    public EffectConfig Effect;

    public abstract SkillRuntime CreateRuntimeSkill(EntityStats owner);
}
