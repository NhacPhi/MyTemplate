using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RangeMagicAttack : SkillRuntime, IAsyncInitializer
{
    private RangeMagicAttackData skillData;

    private GameObject energyBurstPrefab;

    private Entity _caster;
    public RangeMagicAttack(EntityStats owner, RangeMagicAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        await PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.ATTACK);

        await state.WaitForHitFrame();

        DamageFormular.DealDamage(CalculateRawDamage(), caster, enemy);

        energyBurstPrefab.transform.position = caster.Target.transform.position;
        energyBurstPrefab.gameObject.SetActive(true);

        //await UniTask.Delay(1000);
        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        energyBurstPrefab.gameObject.SetActive(false);

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.energyBurstReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            energyBurstPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            energyBurstPrefab.gameObject.SetActive(false);
            //AddressablesManager.Instance.RemoveAsset(objRef);

        }
    }
}

public class RangeMagicAttackData : SkillData
{
    public string energyBurstReference = "Energy_Burst";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new RangeMagicAttack(owner, this);
}


