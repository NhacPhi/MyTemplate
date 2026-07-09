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
        _skillEnd = new UniTaskCompletionSource();
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);
        caster.PlaySFX(skillData.Sound);
        
        // Chờ animation chém tung ra (thời gian có thể chỉnh sửa lại cho khớp với animation của bạn)
        await UniTask.Delay(500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        _caster = caster;
        
        int spawnCount = 3;
        int delayBetweenSpawnsMs = 500;

        for (int i = 0; i < spawnCount; i++)
        {
            Entity currentTarget = caster.Target.GetComponent<Entity>();
            EntityStats targetStats = currentTarget.GetComponent<EntityStats>();

            // Nếu mục tiêu đã chết, tìm mục tiêu mới ra hàng sau
            if (targetStats != null && targetStats.IsDead)
            {
                Entity[] allEntities = Object.FindObjectsOfType<Entity>();
                Entity nextTarget = null;
                foreach (var e in allEntities)
                {
                    var eStats = e.GetComponent<EntityStats>();
                    if (e.Team != caster.Team && eStats != null && !eStats.IsDead)
                    {
                        nextTarget = e;
                        break; // Đã tìm thấy mục tiêu mới
                    }
                }

                if (nextTarget != null)
                {
                    caster.SetTarget(nextTarget);
                }
                else
                {
                    // Quét sạch địch, không còn ai sống -> ngừng bắn
                    break;
                }
            }

            // Tạo một bản sao từ prefab gốc cho mỗi lần bắn
            var bladeInstance = Object.Instantiate(moonBladePrefab, caster.transform);
            bladeInstance.transform.localPosition = skillData.Offset;
            bladeInstance.transform.localScale = new Vector3(5f, 5f, 5f);
            bladeInstance.gameObject.SetActive(true);

            var controller = bladeInstance.GetComponent<FireballController>();

            // Tính toán lại hướng bay theo mục tiêu (có thể đã đổi mục tiêu mới)
            Vector3 flyDir = caster.Target.transform.position - (caster.transform.position + skillData.Offset);

            controller.Initialize(
                caster: caster,
                skill: this,
                direction: flyDir
            );

            // Delay trước khi bắn đợt tiếp theo
            if (i < spawnCount - 1)
            {
                await UniTask.Delay(delayBetweenSpawnsMs, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());
            }
        }

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        // Chờ thêm 800ms để đảm bảo viên đạn cuối cùng (hoặc các viên đạn bay chậm) đã chạm tới mục tiêu.
        // Điều này ngăn chặn lỗi "kết thúc lượt sớm" khi viên 1 vừa chạm, lượt đã chuyển nhưng quái chưa chết,
        // sau đó viên 2-3 đập vào làm quái chết gây kẹt game.
        await UniTask.Delay(800, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        if (!enemy.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(caster, currentTurnID);
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
