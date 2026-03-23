using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class DivineWindSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private DivineWindData skillData;
    private GameObject divineWindPrefab;

    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public DivineWindSkill(EntityStats owner, DivineWindData skillData) : base(owner)
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

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);

        await state.WaitForAnimEnd();
        caster.StateManager.ChangeState(EntityState.IDLE);

        //await UniTask.Delay(1000);

        _caster = caster;

        divineWindPrefab.transform.SetParent(caster.transform);
        divineWindPrefab.transform.localPosition = skillData.Offset;
        divineWindPrefab.transform.localScale = new Vector3(6f, 6f, 6f);

        divineWindPrefab.gameObject.SetActive(true);

        var controller = divineWindPrefab.GetComponent<DivineWindController>();

        Vector3 flyDir = caster.Target.transform.position - caster.transform.position;

        controller.Initialize(
           caster: caster,
           skill: this,
           direction: flyDir
           );

        await _skillEnd.Task;
        await UniTask.Delay(1000);
        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.torandoReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            divineWindPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            divineWindPrefab.gameObject.SetActive(false);
            //AddressablesManager.Instance.RemoveAsset(objRef);

        }
    }

    public void OnDealDamage(ref float damageInput)
    {

    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        HandleDoubleDamageAsync(target).Forget();
    }

    private async UniTask HandleDoubleDamageAsync(Entity target)
    {
        if (target == null) return;

        DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);

        await UniTask.Delay(500, delayTiming: PlayerLoopTiming.Update,
            cancellationToken: target.GetCancellationTokenOnDestroy());

        if (target != null && target.gameObject.activeInHierarchy)
        {
            DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);
        }

        _skillEnd.TrySetResult();
    }
}

public class DivineWindData : SkillData
{
    public Vector3 Offset = new Vector3(-4, 0, 0);

    public string torandoReference = "Divine_Wind";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new DivineWindSkill(owner, this);
}


