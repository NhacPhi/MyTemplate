using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class FrozenBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private FrozenBallData skillData;
    private GameObject frozenBallPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public FrozenBallSkill(EntityStats owner, FrozenBallData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
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

        caster.PlaySFX(skillData.Sound);

        await UniTask.Delay(1800, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        _caster = caster;
        frozenBallPrefab.transform.SetParent(caster.transform);
        frozenBallPrefab.transform.localPosition = skillData.Offset;
        frozenBallPrefab.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        frozenBallPrefab.gameObject.SetActive(true);
        
        Vector3 spawnPos = frozenBallPrefab.transform.position;
        
        // Tách khỏi cha để không bị kéo theo chuyển động của Caster khi bay
        frozenBallPrefab.transform.SetParent(null);

        var controller = frozenBallPrefab.GetComponent<FireballController>();
        Vector3 flyDir = caster.Target.transform.position - spawnPos;

        controller.Initialize(
            caster: caster,
            skill: this,
            direction: flyDir
            );
        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        await _skillEnd.Task;

        await UniTask.Delay(500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.fireBallReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            frozenBallPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            frozenBallPrefab.gameObject.SetActive(false);
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

public class FrozenBallData : SkillData
{
    public Vector3 Offset = new Vector3(-5.9f, -0.94f, 0);

    public string fireBallReference = "FrozenBall";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new FrozenBallSkill(owner, this);
}
