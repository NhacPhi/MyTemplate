using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PoisonBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private PoíonBallData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public PoisonBallSkill(EntityStats owner, PoíonBallData skillData) : base(owner)
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

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);

        await UniTask.Delay(1000);

        _caster = caster;
        firreBallPrefab.transform.SetParent(caster.transform);
        firreBallPrefab.transform.localPosition = skillData.Offset;
        firreBallPrefab.transform.localScale = new Vector3(2.5f,2.5f, 2.5f);
        firreBallPrefab.gameObject.SetActive(true);

        var controller = firreBallPrefab.GetComponent<FireballController>();

        Vector3 flyDir = caster.Target.transform.position - caster.transform.position;

        controller.Initialize(
            caster: caster,
            skill: this,
            direction: flyDir
            );

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        await _skillEnd.Task;

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
            //AddressablesManager.Instance.RemoveAsset(objRef);

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

public class PoíonBallData : SkillData
{
    public Vector3 Offset = new Vector3(6.29f, -0.67f, 0);

    public string fireBallReference = "Poison_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new PoisonBallSkill(owner, this);
}

