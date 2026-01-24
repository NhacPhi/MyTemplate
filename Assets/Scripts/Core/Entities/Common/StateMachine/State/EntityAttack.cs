using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Tech.Pool;

public class EntityAttack : EntityStateBase
{
    protected Action DamageCallBack;
    protected Action ExitCallBack;
    public EntityAttack(EntityStateData data) : base(data)
    {
        DamageCallBack = () =>
        {
            DamageFormular.DealDamage(DamageBonus.GetDefault(), data.Entity, data.CurrentTarget);
        };

        ExitCallBack = () =>
        {
            data.StateManager.ChangeState(EntityState.MOVE_DOWN);
        };
    }

    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.AttackAnimation;
        data.Anim.Play(animData);

        data.Anim.RegisterEventAtTime(0.6f, DamageCallBack);
        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);

    }

}
