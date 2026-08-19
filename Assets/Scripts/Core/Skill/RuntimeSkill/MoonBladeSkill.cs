using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Entity currentTarget = caster.Target != null ? caster.Target.GetComponent<Entity>() : null;

        for (int i = 0; i < spawnCount; i++)
        {
            var targetStats = currentTarget != null ? currentTarget.GetComponent<EntityStats>() : null;

            // Nếu mục tiêu ban đầu/hiện tại không tồn tại hoặc đã bị tiêu diệt -> Tìm mục tiêu ngẫu nhiên mới hợp lệ theo quy tắc hàng & cột
            if (targetStats == null || targetStats.IsDead)
            {
                currentTarget = GetRandomValidTarget(caster);
                targetStats = currentTarget != null ? currentTarget.GetComponent<EntityStats>() : null;
            }

            if (currentTarget == null)
            {
                // Toàn bộ kẻ địch đã bị quét sạch -> ngừng bắn
                break;
            }

            caster.SetTarget(currentTarget);
            caster.HandleTurn(currentTarget); // Xoay người về phía mục tiêu mới

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

    public override DamageBonus CalculateRawDamage()
    {
        var bonus = base.CalculateRawDamage();
        if (bonus.Tags == null) bonus.Tags = new HashSet<string>();
        bonus.Tags.Add("UltimateSkill");
        return bonus;
    }

    public void OnDealDamage(ref float damageInput)
    {

    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);

        _skillEnd.TrySetResult();
    }

    private Entity GetRandomValidTarget(Entity caster)
    {
        List<Entity> opponentTeam = null;

        if (BattleManager.Instance != null)
        {
            opponentTeam = caster.Team == TeamSide.Player 
                ? BattleManager.Instance.Enemies 
                : BattleManager.Instance.Characters.Values.ToList();
        }
        else
        {
            var allEntities = Object.FindObjectsOfType<Entity>();
            opponentTeam = new List<Entity>();
            foreach (var e in allEntities)
            {
                if (e.Team != caster.Team) opponentTeam.Add(e);
            }
        }

        if (opponentTeam == null || opponentTeam.Count == 0) return null;

        TargetManager targetManager = BattleManager.Instance != null && BattleManager.Instance.TargetSystem != null
            ? BattleManager.Instance.TargetSystem
            : new TargetManager();

        List<Entity> validTargets = targetManager.GetValidEtitiesByColumnLogic(opponentTeam);

        if (validTargets == null || validTargets.Count == 0) return null;

        int randomIndex = Random.Range(0, validTargets.Count);
        return validTargets[randomIndex];
    }
}

public class MoonBladeData : SkillData
{
    public Vector3 Offset = new Vector3(4.09f, -1.06f, 0);

    // Đây là tên Addressable của Prefab chứa hiệu ứng MoonBlade
    public string moonBladeReference = "MoonBlade";

    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MoonBladeSkill(owner, this);
}
