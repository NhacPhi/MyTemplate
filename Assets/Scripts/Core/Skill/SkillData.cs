using UnityEngine;

public abstract class SkillData
{
    private string id;

    public string ID { get; protected set; }

    public abstract SkillRuntime CreateRuntimeSkill(EntityStats owner);
}
