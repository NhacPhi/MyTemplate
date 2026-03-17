using Cysharp.Threading.Tasks;
using System;
using Tech.Pool;

public class EntityMainSkill : EntityStateBase
{
    protected Action HitCallBack;
    protected Action ExitCallBack;
    public EntityMainSkill(EntityStateData data) : base(data)
    {
        HitCallBack = () => data.TriggerHitFrame();
        ExitCallBack = () => data.TriggerAnimEnd();
    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.MainSkillAnimaiton;
        data.Anim.Play(animData);

        data.Anim.RegisterEventAtTime(data.TimeTriggerDamge, HitCallBack);
        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);
    }



    //protected virtual async UniTaskVoid WaitToAttack()
    //{
    //    await UniTask.WaitForSeconds(1.3f, cancellationToken: data.Token);
    //    data.StateManager.ChangeState(EntityState.MOVE_DOWN);
    //}
}
