using System;
using System.Threading;
using Tech.Pool;

public class EntityMajorSkill : EntityStateBase
{
    protected Action ExitCallBack;

    public EntityMajorSkill(EntityStateData data) : base(data)
    {
        ExitCallBack = () =>
        {
            data.StateManager.ChangeState(EntityState.IDLE);
        };
    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.MajorAnimaiton;
        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);

        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);
    }

}
