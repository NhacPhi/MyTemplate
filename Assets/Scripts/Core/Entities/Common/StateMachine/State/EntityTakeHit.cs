using System;
using Tech.Pool;
public class EntityTakeHit : EntityStateBase
{
    public EntityTakeHit(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.ParameterName = data.HitParam;
        animData.ParameterType = AnimatorParameterType.Trigger;
        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);

        data.StateManager.ChangeState(EntityState.IDLE);
    }
}
