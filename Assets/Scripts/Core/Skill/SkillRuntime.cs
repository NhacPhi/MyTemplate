using Cysharp.Threading.Tasks;
using System;

public abstract class SkillRuntime
{
    protected EntityStats owner;

    public SkillRuntime(EntityStats owner)
    {
        this.owner = owner;
    }

    public abstract SkillData GetSkillData();

    public abstract UniTask ExecuteAsync(Entity caster);
}
