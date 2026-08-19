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
            // Chỉ trở về IDLE nếu hiện tại vẫn đang ở đúng state HIT (tránh ghi đè khi nhân vật đã sang MOVE_UP/ATTACK)
            if (data.StateManager != null && data.StateManager.CurrentState == this)
            {
                data.StateManager.ChangeState(EntityState.IDLE);
            }
        });
    }

    // Register event Onhit
    protected async UniTaskVoid WaitInit()
    {
        await UniTask.Yield();
        var stats = data.Entity != null ? data.Entity.GetComponent<EntityStats>() : null;
        if (stats != null)
        {
            stats.OnHit += (_, _, tags) =>
            {
                // Sát thương DoT (Độc, Cháy, Chảy máu...) không làm giật stagger animation hoặc đổi state
                if (tags != null && (tags.Contains("DoT") || tags.Contains("Poison") || tags.Contains("Burn") || tags.Contains("Bleed")))
                {
                    return;
                }

                if (data.StateManager != null)
                {
                    data.StateManager.ChangeState(EntityState.HIT);
                }
            };
        }
    }
}
