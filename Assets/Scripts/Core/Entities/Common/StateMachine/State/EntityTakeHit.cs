using System;
using Tech.Pool;
using Cysharp.Threading.Tasks;
public class EntityTakeHit : EntityStateBase
{
    public EntityTakeHit(EntityStateData data) : base(data)
    {
        _ = WaitInit();
    }

    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.HitAnimation;
        animData.Transition = 0.1f;

        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);

        data.Anim.RegisterEventAtTime(0.9f, () =>
        {
            data.StateManager.ChangeState(EntityState.IDLE);
        });
    }

    // Register event Onhit
    protected async UniTaskVoid WaitInit()
    {
        await UniTask.Yield();
        data.Entity.GetComponent<EntityStats>().OnHit += (_, _) =>
        {
            data.StateManager.ChangeState(EntityState.HIT);
        };
    }
}
