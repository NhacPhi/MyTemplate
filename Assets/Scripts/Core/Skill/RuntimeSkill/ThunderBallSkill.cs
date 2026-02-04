using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;


public class ThunderBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private ThunderBallData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;
    public ThunderBallSkill(EntityStats owner, ThunderBallData skillData) : base(owner)
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
        firreBallPrefab.transform.SetParent(caster.transform);
        firreBallPrefab.transform.localPosition = skillData.Offset;
        firreBallPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
        firreBallPrefab.gameObject.SetActive(true);

        var controller = firreBallPrefab.GetComponent<ThunderBallController>();

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

public class ThunderBallData : SkillData
{
    public Vector3 Offset = new Vector3(3.14f, 1.5f, 0);

    public string fireBallReference = "Thunder_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new ThunderBallSkill(owner, this);
}
