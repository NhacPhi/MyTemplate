using UnityEngine;

public abstract class SkillData
{
    private string id;

    public string ID { get; protected set; }

    public float DamageMultiplier;

    public int MaxCoolDown;

    public float FlatDamage;

    public abstract SkillRuntime CreateRuntimeSkill(EntityStats owner);
}
