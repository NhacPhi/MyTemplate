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
    private UniTaskCompletionSource _skillEnd;
    public TorandoSkill(EntityStats owner, TorandoData skillData) : base(owner)
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
        await state.WaitForAnimEnd();
        caster.StateManager.ChangeState(EntityState.IDLE);
        await _skillEnd.Task;
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

        PutOnCooldown();
    }
}

public class TorandoData : SkillData
{
    public Vector3 Offset = new Vector3(-3, 0, 0);

    public string torandoReference = "Torando";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new TorandoSkill(owner, this);
}

