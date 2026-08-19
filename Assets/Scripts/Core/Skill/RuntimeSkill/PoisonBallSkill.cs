using Cysharp.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PoisonBallSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private PoisonBallData skillData;
    private GameObject firreBallPrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    public PoisonBallSkill(EntityStats owner, PoisonBallData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        await PerformSummon(skillData, caster, currentTurnID);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster,int currentTurnID)
    {
        _skillEnd = new UniTaskCompletionSource();
        var enemy = caster.Target != null ? caster.Target.gameObject.GetComponent<Entity>() : null;
        if (enemy != null) caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);
        caster.PlaySFX(skillData.Sound);
        await UniTask.Delay(1500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        _caster = caster;
        if (firreBallPrefab != null && caster.Target != null)
        {
            firreBallPrefab.transform.SetParent(caster.transform);
            Vector3 spawnOffset = skillData.Offset;
            Vector3 scale = new Vector3(2.5f, 2.5f, 2.5f);
            if (caster.Team == TeamSide.Enemy)
            {
                spawnOffset.x *= -1f;
                scale.y *= -1f; // Do Prefab bị xoay Z = 90 nên lật trục Y sẽ tương đương lật ngang
            }
            firreBallPrefab.transform.localPosition = spawnOffset;
            firreBallPrefab.transform.localScale = scale;
            firreBallPrefab.gameObject.SetActive(true);

            var controller = firreBallPrefab.GetComponent<FireballController>();
            if (controller != null)
            {
                Vector3 flyDir = caster.Target.transform.position - firreBallPrefab.transform.position;
                controller.Initialize(
                    caster: caster,
                    skill: this,
                    direction: flyDir
                );
            }
        }
        else if (enemy != null)
        {
            // Fallback gây sát thương trực tiếp nếu không có prefab đạn
            DamageFormular.DealDamage(CalculateRawDamage(), caster, enemy);
            _skillEnd.TrySetResult();
        }

        if (state != null)
        {
            await state.WaitForAnimEnd();
        }

        caster.StateManager.ChangeState(EntityState.IDLE);

        // Chờ đạn chạm mục tiêu hoặc timeout an toàn sau 2.5s để không bao giờ bị treo game
        await UniTask.WhenAny(_skillEnd.Task, UniTask.Delay(2500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy()));

        if (enemy != null && enemy.GetCoreComponent<EntityStats>() != null && !enemy.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(caster, currentTurnID);
        }

        PutOnCooldown();
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

public class PoisonBallData : SkillData
{
    public Vector3 Offset = new Vector3(6.29f, -0.67f, 0);

    public string fireBallReference = "Poison_Ball";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new PoisonBallSkill(owner, this);
}

