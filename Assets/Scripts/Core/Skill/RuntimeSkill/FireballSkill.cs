using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class FireballSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private FireBallData skillData;
    private GameObject fireBallPrefab;
    private Entity _caster;
    private UniTaskCompletionSource _skillEnd;
    private int _currentTurnID;

    public FireballSkill(EntityStats owner, FireBallData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override DamageBonus CalculateRawDamage()
    {
        var bonus = base.CalculateRawDamage();
        if (bonus.Tags == null) bonus.Tags = new System.Collections.Generic.HashSet<string>();
        bonus.Tags.Add("MajorSkill");
        return bonus;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        _skillEnd = new UniTaskCompletionSource();
        _caster = caster;
        _currentTurnID = currentTurnID;

        if (fireBallPrefab != null)
        {
            fireBallPrefab.transform.SetParent(caster.transform);
            fireBallPrefab.transform.localPosition = skillData.Offset;
            fireBallPrefab.transform.localScale = new Vector3(1.4f, -1.4f, 1.4f);
            fireBallPrefab.gameObject.SetActive(true);

            var controller = fireBallPrefab.GetComponent<FireballController>();

            Vector3 flyDir = caster.Target.transform.position - caster.transform.position;
            caster.PlaySFX(skillData.Sound);
            controller.Initialize(
                caster: caster,
                skill: this,
                direction: flyDir
            );
        }
        else if (caster.Target != null)
        {
            var targetEntity = caster.Target.GetComponent<Entity>();
            DamageFormular.DealDamage(CalculateRawDamage(), caster, targetEntity);
            if (targetEntity != null && targetEntity.GetCoreComponent<EntityStats>() != null && !targetEntity.GetCoreComponent<EntityStats>().IsDead)
            {
                ApplyEffectsToTarget(caster, currentTurnID);
            }
            _skillEnd.TrySetResult();
        }

        await UniTask.WhenAny(_skillEnd.Task, UniTask.Delay(2500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy()));

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.fireBallReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            if (ring != null)
            {
                fireBallPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
                fireBallPrefab.gameObject.SetActive(false);
            }
        }
    }

    public void OnDealDamage(ref float damageInput)
    {
        
    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);

        if (target != null && target.GetCoreComponent<EntityStats>() != null && !target.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(_caster, _currentTurnID);
        }

        _skillEnd.TrySetResult();
    }
}

public class FireBallData : SkillData
{
    public Vector3 Offset = new Vector3(1.16f, -1f, 0);

    public string fireBallReference = "Fire_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new FireballSkill(owner, this);
}

