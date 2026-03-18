using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;


public class ThunderBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private ThunderBallData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public ThunderBallSkill(EntityStats owner, ThunderBallData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster)
    {
        await PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        _skillEnd = new UniTaskCompletionSource();

        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);

        await UniTask.Delay(1000);

        _caster = caster;
        firreBallPrefab.transform.SetParent(caster.transform);
        firreBallPrefab.transform.localPosition = skillData.Offset;
        firreBallPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
        firreBallPrefab.gameObject.SetActive(true);

        var controller = firreBallPrefab.GetComponent<ThunderBallController>();

        var vec = new Vector3(caster.transform.position.x + skillData.Offset.x, caster.transform.position.y + skillData.Offset.y, caster.transform.position.z);

        Vector3 flyDir = caster.Target.transform.position - vec;

        controller.Initialize(
            caster: caster,
            skill: this,
            direction: flyDir
            );

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        await _skillEnd.Task;

        await UniTask.Delay(500);

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.fireBallReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            firreBallPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            firreBallPrefab.gameObject.SetActive(false);
            AddressablesManager.Instance.RemoveAsset(objRef);

        }
    }

    public void OnDealDamage(ref float damageInput)
    {

    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);

        _skillEnd.TrySetResult();
    }
}

public class ThunderBallData : SkillData
{
    public Vector3 Offset = new Vector3(3.14f, 1.5f, 0);

    public string fireBallReference = "Thunder_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new ThunderBallSkill(owner, this);
}
