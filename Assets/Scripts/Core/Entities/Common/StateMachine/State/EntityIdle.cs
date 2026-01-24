using Tech.Pool;

public class EntityIdle : EntityStateBase
{
   public EntityIdle(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
        // handle Animation
        var animData = GenericPool<AnimationData>.Get().Renew();
        animData.AnimationName = data.IdleAnimation;
        data.Anim.Play(animData);
        GenericPool<AnimationData>.Return(animData);
    }
}
