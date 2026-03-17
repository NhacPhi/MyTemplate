using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Tech.Pool;

public class EntityAttack : EntityStateBase
{
    protected Action HitCallBack;
    protected Action ExitCallBack;
    public EntityAttack(EntityStateData data) : base(data)
    {
        HitCallBack = () => data.TriggerHitFrame();
        ExitCallBack = () => data.TriggerAnimEnd();
    }

    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.AttackAnimation;
        data.Anim.Play(animData);

        data.Anim.RegisterEventAtTime(0.6f, HitCallBack);
        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);

    }

}
