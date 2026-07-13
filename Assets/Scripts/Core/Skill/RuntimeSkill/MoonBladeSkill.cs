using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MoonBladeSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private MoonBladeData skillData;
    private GameObject moonBladePrefab;
    private Entity _caster;

    private UniTaskCompletionSource _skillEnd;
    
    public MoonBladeSkill(EntityStats owner, MoonBladeData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        await PerformSummon(skillData, caster, currentTurnID);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster, int currentTurnID)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);
        caster.PlaySFX(skillData.Sound);
        
        // Chờ animation chém tung ra
        await UniTask.Delay(300, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        _caster = caster;
        
        int spawnCount = 3;
        bool allDead = false;

        for (int i = 0; i < spawnCount; i++)
        {
            Entity currentTarget = caster.Target != null ? caster.Target.GetComponent<Entity>() : null;
            EntityStats targetStats = currentTarget != null ? currentTarget.GetComponent<EntityStats>() : null;

            // Kiểm tra máu thực tế của mục tiêu (không cần máu ảo nữa vì các viên đạn trước đã xử lý xong)
            if (targetStats == null || targetStats.IsDead)
            {
                Entity[] allEntities = Object.FindObjectsOfType<Entity>();
                Entity nextTarget = null;
                foreach (var e in allEntities)
                {
                    var eStats = e.GetComponent<EntityStats>();
                    if (e.Team != caster.Team && eStats != null && !eStats.IsDead)
                    {
                        nextTarget = e;
                        break; 
                    }
                }

                if (nextTarget != null)
                {
                    caster.SetTarget(nextTarget);
                    caster.HandleTurn(nextTarget); // Xoay người sang đích mới ngay lập tức
                    currentTarget = nextTarget;
                    targetStats = nextTarget.GetComponent<EntityStats>();
                }
                else
                {
                    // Quét sạch địch, không còn ai sống -> ngừng bắn
                    allDead = true;
                    break;
                }
            }

            // Tạo Task mới cho mỗi viên đạn
            _skillEnd = new UniTaskCompletionSource();

            var bladeInstance = Object.Instantiate(moonBladePrefab, caster.transform);
            Vector3 spawnOffset = skillData.Offset;
            Vector3 scale = new Vector3(5f, 5f, 5f);
            if (caster.Team == TeamSide.Enemy)
            {
                spawnOffset.x *= -1f;
            }
            bladeInstance.transform.localPosition = spawnOffset;
            bladeInstance.transform.localScale = scale;
            
            // Cắt liên kết cha-con để khi nhân vật quay đầu (HandleTurn), đạn không bị bay cong theo
            bladeInstance.transform.SetParent(null);
            bladeInstance.gameObject.SetActive(true);

            var controller = bladeInstance.GetComponent<FireballController>();

            Vector3 startPos = bladeInstance.transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            var targetCollider = currentTarget.GetComponent<Collider>();
            if (targetCollider != null) targetPos = targetCollider.bounds.center;

            Vector3 flyDir = targetPos - startPos;
            
            // Quay đạn theo hướng bay
            float angle = Mathf.Atan2(flyDir.y, flyDir.x) * Mathf.Rad2Deg;
            bladeInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

            controller.Initialize(
                caster: caster,
                skill: this,
                direction: flyDir
            );
            
            // ĐỢI VIÊN ĐẠN NÀY CHẠM ĐÍCH HOẶC TỐI ĐA 2 GIÂY TRƯỚC KHI BẮN VIÊN TIẾP THEO!
            await UniTask.WhenAny(_skillEnd.Task, UniTask.Delay(2000, cancellationToken: caster.transform.GetCancellationTokenOnDestroy()));
        }

        caster.StateManager.ChangeState(EntityState.IDLE);

        // Áp dụng các hiệu ứng (Debuff, Dot...) lên mục tiêu cuối cùng nếu nó còn sống

        Entity finalTarget = caster.Target != null ? caster.Target.GetComponent<Entity>() : null;
        if (finalTarget != null)
        {
            var finalStats = finalTarget.GetCoreComponent<EntityStats>();
            if (finalStats != null && !finalStats.IsDead)
            {
                ApplyEffectsToTarget(caster, currentTurnID);
            }
        }

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.moonBladeReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            moonBladePrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            moonBladePrefab.gameObject.SetActive(false);
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

public class MoonBladeData : SkillData
{
    public Vector3 Offset = new Vector3(4.09f, -1.06f, 0);

    // Đây là tên Addressable của Prefab chứa hiệu ứng MoonBlade
    public string moonBladeReference = "MoonBlade";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MoonBladeSkill(owner, this);
}
