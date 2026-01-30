using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FireballSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private FireballData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;
    public FireballSkill(EntityStats owner, FireballData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override void Execute(Entity caster)
    {
        _caster = caster;
        firreBallPrefab.transform.SetParent(caster.transform);
        firreBallPrefab.transform.localPosition = skillData.Offset;
        firreBallPrefab.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
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

public class FireballData : SkillData
{
    public Vector3 Offset = new Vector3(1.16f, -1f, 0);

    public string fireBallReference = "Fire_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new FireballSkill(owner, this);
}

