using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SurikenSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private SurikenData skillData;
    private GameObject surikenPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public SurikenSkill(EntityStats owner, SurikenData skillData) : base(owner)
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

        await UniTask.Delay(600);

        _caster = caster;

        surikenPrefab.transform.SetParent(caster.transform);
        surikenPrefab.transform.localPosition = skillData.Offset;
        surikenPrefab.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        surikenPrefab.gameObject.SetActive(true);

        var controller = surikenPrefab.GetComponent<FireballController>();

        Vector3 flyDir = caster.Target.transform.position - caster.transform.position;

        controller.Initialize(
           caster: caster,
           skill: this,
           direction: flyDir
           );


        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        await _skillEnd.Task;

        await UniTask.Delay(600);

        PutOnCooldown();
    }
    public override SkillData GetSkillData() => skillData;
    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.surikenRefernce;

        if (objRef != null)
        {
            GameObject suriken = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            surikenPrefab = Object.Instantiate(suriken, Vector3.zero, suriken.transform.rotation);
            surikenPrefab.gameObject.SetActive(false);
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

public class SurikenData : SkillData
{
    public Vector3 Offset = new Vector3(-5f, 0f, 0);

    public string surikenRefernce = "Suriken";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new SurikenSkill(owner, this);
}

