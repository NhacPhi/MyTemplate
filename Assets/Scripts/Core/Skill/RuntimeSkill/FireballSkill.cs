using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class FireballSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private FireBallData skillData;
    private GameObject fireBallPrefab;
    private Entity _caster;
    private UniTaskCompletionSource _skillEnd;
    public FireballSkill(EntityStats owner, FireBallData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        _skillEnd = new UniTaskCompletionSource();
        _caster = caster;
        fireBallPrefab.transform.SetParent(caster.transform);
        fireBallPrefab.transform.localPosition = skillData.Offset;
        fireBallPrefab.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        fireBallPrefab.gameObject.SetActive(true);

        var controller = fireBallPrefab.GetComponent<FireballController>();

        Vector3 flyDir = caster.Target.transform.position - caster.transform.position;

        controller.Initialize(
            caster: caster,
            skill: this,
            direction: flyDir
            );

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
            fireBallPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            fireBallPrefab.gameObject.SetActive(false);
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

public class FireBallData : SkillData
{
    public Vector3 Offset = new Vector3(1.16f, -1f, 0);

    public string fireBallReference = "Fire_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new FireballSkill(owner, this);
}

