using Cysharp.Threading.Tasks;
using System;
using Tech.Pool;
using Unity.VisualScripting.Antlr3.Runtime;

public class EntityAttack : EntityStateBase
{
    protected Action DamageCallBack;
    protected Action ExitCallBack;
    public EntityAttack(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.ParameterName = data.AttackParam;
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
