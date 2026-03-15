using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TorandoSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private TorandoData skillData;
    private GameObject torandoPrefab;

    private Entity _caster;
    public TorandoSkill(EntityStats owner, TorandoData skillData) : base(owner)
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
            stateData.StateManager.ChangeState(EntityState.MAJOR_SKILL);
            //state.StateManager.ChangeState(EntityState.MOVE_UP);
        }

        await UniTask.Delay(1000);

        _caster = caster;

        torandoPrefab.transform.SetParent(caster.transform);
        torandoPrefab.transform.localPosition = skillData.Offset;
        //torandoPrefab.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        torandoPrefab.gameObject.SetActive(true);

        var controller = torandoPrefab.GetComponent<TorandoController>();

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
            torandoPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            torandoPrefab.gameObject.SetActive(false);
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

public class TorandoData : SkillData
{
    public Vector3 Offset = new Vector3(3, 0, 0);

    public string torandoReference = "Torando";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new TorandoSkill(owner, this);
}

