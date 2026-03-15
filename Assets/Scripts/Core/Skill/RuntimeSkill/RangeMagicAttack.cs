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

    public override void Execute(Entity caster)
    {
        _ = PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        var enemy_ultimate = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy_ultimate, false);

        energyBurstPrefab.gameObject.SetActive(false);
        await UniTask.Delay(1000);
        energyBurstPrefab.transform.position = caster.Target.transform.position;
        energyBurstPrefab.gameObject.SetActive(true);
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
            AddressablesManager.Instance.RemoveAsset(objRef);

        }
    }
}

public class RangeMagicAttackData : SkillData
{
    public string energyBurstReference = "Energy_Burst";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new RangeMagicAttack(owner, this);
}


