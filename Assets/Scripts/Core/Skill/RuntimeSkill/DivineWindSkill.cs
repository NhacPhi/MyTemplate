using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DivineWindSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private DivineWindData skillData;
    private GameObject divineWindPrefab;

    private Entity _caster;
    public DivineWindSkill(EntityStats owner, DivineWindData skillData) : base(owner)
    {
        this.skillData = skillData;
    }
    public override void Execute(Entity caster)
    {
        _ = PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        EntityStateData stateData = caster.GetComponent<EntityStateData>();

        if (stateData != null)
        {
            stateData.StateManager.ChangeState(EntityState.MAIN_SKILL);
            //state.StateManager.ChangeState(EntityState.MOVE_UP);
        }

        await UniTask.Delay(1000);

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
            AddressablesManager.Instance.RemoveAsset(objRef);

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

        var damage = new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1.5f
        };

        DamageFormular.DealDamage(damage, _caster, target);

        await UniTask.Delay(500, delayTiming: PlayerLoopTiming.Update,
            cancellationToken: target.GetCancellationTokenOnDestroy());

        if (target != null && target.gameObject.activeInHierarchy)
        {
            DamageFormular.DealDamage(damage, _caster, target);
        }
    }
}

public class DivineWindData : SkillData
{
    public Vector3 Offset = new Vector3(4, 0, 0);

    public string torandoReference = "Divine_Wind";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new DivineWindSkill(owner, this);
}


