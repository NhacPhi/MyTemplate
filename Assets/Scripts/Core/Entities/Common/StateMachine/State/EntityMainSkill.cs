using Cysharp.Threading.Tasks;
using System;
using Tech.Pool;

public class EntityMainSkill : EntityStateBase
{
    protected Action DamageCallBack;
    protected Action ExitCallBack;
    public EntityMainSkill(EntityStateData data) : base(data)
    {
        DamageCallBack = () =>
        {
            DamageFormular.DealDamage(new DamageBonus() {
                FlatValue = 0,
                DamageMultiplier = 2f
            }, data.Entity, data.CurrentTarget);
        };

        ExitCallBack = () =>
        {
            data.StateManager.ChangeState(EntityState.MOVE_DOWN);
        };
    }


    public override void Enter()
    {
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.MainSkillAnimaiton;
        data.Anim.Play(animData);


        data.Anim.RegisterEventAtTime(0.6f, DamageCallBack);
        data.Anim.RegisterEventAtTime(0.95f, ExitCallBack);
    }



    //protected virtual async UniTaskVoid WaitToAttack()
    //{
    //    await UniTask.WaitForSeconds(1.3f, cancellationToken: data.Token);
    //    data.StateManager.ChangeState(EntityState.MOVE_DOWN);
    //}
}
