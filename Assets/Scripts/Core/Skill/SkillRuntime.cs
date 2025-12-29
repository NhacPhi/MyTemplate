using System;

public abstract class SkillRuntime
{
    protected EntityStats owner;

    public SkillRuntime(EntityStats owner)
    {
        this.owner = owner;
    }

    public abstract SkillData GetSkillData();

    public abstract void Execute(Entity caster);
}
