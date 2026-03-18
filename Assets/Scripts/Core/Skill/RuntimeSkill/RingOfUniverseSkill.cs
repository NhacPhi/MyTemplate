using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class RingOfUniverseSkill : SkillRuntime, IAttackSkill, IReturningProjectileSkill, IAsyncInitializer
{
    private RingOfUniverseData skillData;
    private GameObject ringPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;

    public RingOfUniverseSkill(EntityStats owner, RingOfUniverseData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster)
    {
        _caster = caster;
       
        await ThrowProjectile(skillData, caster);
    }

    public override SkillData GetSkillData() => skillData;


    public void OnDealDamage(ref float damageInput)
    {
        
    }

    public GameObject ActiveProjectiles => throw new System.NotImplementedException();

    public void OnProjectileHit(Entity target, GameObject projectile)
    {
        DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);
    }

    public void OnProjectileReturned(GameObject projectitle)
    {
        ringPrefab.gameObject.SetActive(false);
        _skillEnd.TrySetResult();
    }

    public async UniTask ThrowProjectile(SkillData data, Entity caster)
    {
        _skillEnd = new UniTaskCompletionSource();

        ringPrefab.transform.SetParent(caster.transform);
        ringPrefab.transform.localPosition = skillData.Offset;
        ringPrefab.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        ringPrefab.gameObject.SetActive(true);

        var controller = ringPrefab.GetComponent<ProjectileController>();

        Vector3 throwDir = caster.Target.transform.position - caster.transform.position;

        float maxDis = Vector3.Distance(caster.transform.position, caster.Target.transform.position) - 1f;
        controller.Initialize(
            caster: caster,
            skill: this,
            direction: throwDir,
            maxDist: maxDis
            );

        await _skillEnd.Task;

        PutOnCooldown();
    }

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var vfxRef = skillData.ringObjectReference;

        if (vfxRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(vfxRef);
            ringPrefab = Object.Instantiate(ring, Vector3.zero, Quaternion.identity);
            ringPrefab.gameObject.SetActive(false);
            AddressablesManager.Instance.RemoveAsset(vfxRef);

        }
    }
}


public class RingOfUniverseData: SkillData
{
    public Vector3 Offset = new Vector3(1.68f, -1.43f, 0);

    public string ringObjectReference = "Ring_Of_Universe";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new RingOfUniverseSkill(owner, this);
}
