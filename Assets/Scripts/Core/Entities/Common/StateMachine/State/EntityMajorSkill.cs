using System;
using System.Threading;
using Tech.Pool;

public class EntityMajorSkill : EntityStateBase
{
    protected Action ExitCallBack;
    protected Action DamageCallBack;
    public EntityMajorSkill(EntityStateData data) : base(data)
    {
        DamageCallBack = () =>
        {
            DamageFormular.DealDamage(new DamageBonus()
            {
                FlatValue = 0,
                DamageMultiplier = 1.5f
            }, data.Entity, data.CurrentTarget);
        };
        ExitCallBack = () =>
        {
            data.StateManager.ChangeState(EntityState.MOVE_DOWN);
        };
    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.MajorAnimaiton;
        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);

        if(data.IsMajorAttack)
            data.Anim.RegisterEventAtTime(data.RadioTimeTriggerDamgeMajor, DamageCallBack);

        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);
    }

}
