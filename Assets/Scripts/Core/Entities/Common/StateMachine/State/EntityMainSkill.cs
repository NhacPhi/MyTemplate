using Cysharp.Threading.Tasks;
using System;
using Tech.Pool;

public class EntityMainSkill : EntityStateBase
{
    public EntityMainSkill(EntityStateData data) : base(data)
    {

    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.ParameterName = data.MainSkillParam;
        animData.ParameterType = AnimatorParameterType.Trigger;
        data.Anim.Play(animData);

        _ = WaitToAttack();
    }



    protected virtual async UniTaskVoid WaitToAttack()
    {
        await UniTask.WaitForSeconds(data.TimeToAttack, cancellationToken: data.Token);
        data.StateManager.ChangeState(EntityState.MOVE_DOWN);
    }
}
