using System;
using System.Threading;
using Tech.Pool;

public class EntityMajorSkill : EntityStateBase
{
    protected Action HitCallBack;
    protected Action ExitCallBack;
    public EntityMajorSkill(EntityStateData data) : base(data)
    {
        HitCallBack = () => data.TriggerHitFrame();
        ExitCallBack = () => data.TriggerAnimEnd();
    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.MajorAnimaiton;
        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);

        //if(data.IsMajorAttack)
        //    data.Anim.RegisterEventAtTime(data.RadioTimeTriggerDamgeMajor, HitCallBack);

        data.Anim.RegisterEventAtTime(data.RadioTimeTriggerDamgeMajor, HitCallBack);
        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);
    }
}
