using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PoisonBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private PosionBallData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;
    public PoisonBallSkill(EntityStats owner, PosionBallData skillData) : base(owner)
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
        firreBallPrefab.transform.SetParent(caster.transform);
        firreBallPrefab.transform.localPosition = skillData.Offset;
        firreBallPrefab.transform.localScale = new Vector3(2.5f,2.5f, 2.5f);
        firreBallPrefab.gameObject.SetActive(true);

        var controller = firreBallPrefab.GetComponent<FireballController>();

        Vector3 flyDir = caster.target.transform.position - caster.transform.position;

        controller.Initialize(
            caster: caster,
            skill: this,
            direction: flyDir
            );
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
        var damage = new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1.5f
        };
        DamageFormular.DealDamage(damage, _caster, target);
    }
}

public class PosionBallData : SkillData
{
    public Vector3 Offset = new Vector3(6.29f, -0.67f, 0);

    public string fireBallReference = "Poison_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new PoisonBallSkill(owner, this);
}

