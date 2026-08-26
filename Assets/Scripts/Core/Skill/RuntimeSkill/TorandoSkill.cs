using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TorandoSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private TorandoData skillData;
    private GameObject torandoPrefab;
    private List<GameObject> activeTornadoClones = new List<GameObject>();

    private Entity _caster;
    private UniTaskCompletionSource _skillEnd;
    private int _currentTurnID;
    private int _pendingHitsCount = 0;

    public TorandoSkill(EntityStats owner, TorandoData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        _currentTurnID = currentTurnID;
        await PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        _skillEnd = new UniTaskCompletionSource();
        _caster = caster;

        var mainTarget = caster.Target != null ? caster.Target.GetComponent<Entity>() : null;
        if (mainTarget != null)
        {
            caster.HandleTurn(mainTarget);
        }

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);
        caster.PlaySFX(skillData.Sound);
        if (state != null)
        {
            await state.WaitForAnimEnd();
        }
        caster.StateManager.ChangeState(EntityState.IDLE);

        // 1. Xác định danh sách mục tiêu (Hỗ trợ 3 mục tiêu theo quy luật hàng cột nếu là EnemyRow)
        List<Entity> chosenTargets = new List<Entity>();
        if (skillData.TargetType == SkillTargetType.EnemyRow || skillData.TargetType == SkillTargetType.AllEnemies)
        {
            chosenTargets = GetThreeTargetsByColumnLogic(caster, mainTarget);
        }
        if (chosenTargets == null || chosenTargets.Count == 0)
        {
            if (mainTarget != null) chosenTargets.Add(mainTarget);
        }

        _pendingHitsCount = chosenTargets.Count;

        // 2. Tạo các cơn lốc xoáy (VFX) phóng tới từng kẻ địch đồng thời
        for (int i = 0; i < chosenTargets.Count; i++)
        {
            var target = chosenTargets[i];
            if (target == null) continue;

            GameObject tornado = null;
            if (i == 0)
            {
                tornado = torandoPrefab;
            }
            else
            {
                tornado = Object.Instantiate(torandoPrefab, caster.transform.position, torandoPrefab.transform.rotation);
                activeTornadoClones.Add(tornado);
            }

            tornado.transform.SetParent(caster.transform);
            // Độ lệch vị trí hình quạt giữa các lốc xuất phát
            Vector3 offset = skillData.Offset + new Vector3(0f, (i - 1) * 0.45f, (i - 1) * 0.35f);
            tornado.transform.localPosition = offset;
            tornado.transform.localScale = _originalScale;
            tornado.gameObject.SetActive(true);

            var controller = tornado.GetComponent<TorandoController>();
            if (controller != null)
            {
                Vector3 flyDir = target.transform.position - tornado.transform.position;
                controller.Initialize(caster, this, flyDir, target);
            }
        }

        await _skillEnd.Task;
        await UniTask.Delay(500, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

        // 3. Dọn dẹp các clone lốc sau khi hoàn tất
        foreach (var clone in activeTornadoClones)
        {
            if (clone != null)
            {
                Object.Destroy(clone);
            }
        }
        activeTornadoClones.Clear();

        PutOnCooldown();
    }

    private List<Entity> GetThreeTargetsByColumnLogic(Entity caster, Entity mainTarget)
    {
        List<Entity> targets = new List<Entity>();

        if (mainTarget != null && !mainTarget.GetCoreComponent<EntityStats>().IsDead)
        {
            targets.Add(mainTarget);
        }

        var opposingTeam = caster.Team == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player;
        List<Entity> allOpponents = BattleManager.Instance != null 
            ? BattleManager.Instance.GetEntitiesByTeam(opposingTeam).Where(e => e != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList()
            : new List<Entity>();

        // Lấy mục tiêu ưu tiên theo quy luật Hàng - Cột
        var targetManager = new TargetManager();
        var columnTargets = targetManager.GetValidEtitiesByColumnLogic(allOpponents);

        foreach (var t in columnTargets)
        {
            if (!targets.Contains(t) && targets.Count < 3)
            {
                targets.Add(t);
            }
        }

        // Nếu chưa đủ 3 mục tiêu, lấy thêm các đối thủ còn sống khác
        foreach (var t in allOpponents)
        {
            if (!targets.Contains(t) && targets.Count < 3)
            {
                targets.Add(t);
            }
        }

        return targets;
    }

    public override SkillData GetSkillData() => skillData;

    private Vector3 _originalScale = new Vector3(8f, 8f, 8f);

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var objRef = skillData.torandoReference;

        if (objRef != null)
        {
            GameObject ring = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(objRef);
            _originalScale = ring.transform.localScale;
            torandoPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            torandoPrefab.gameObject.SetActive(false);
            //AddressablesManager.Instance.RemoveAsset(objRef);

        }
    }

    public void OnDealDamage(ref float damageInput)
    {
    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        if (target != null && target.gameObject.activeInHierarchy && !target.GetCoreComponent<EntityStats>().IsDead)
        {
            // Áp đặt hiệu ứng Ngân Ấn (hoặc effect cấu hình) lên mục tiêu bị trúng lốc
            var targetStats = target.GetCoreComponent<StatsController>();
            if (targetStats != null && GetSkillData().Effect != null)
            {
                StatusEffect newEffect = EffectFactory.CreateEffect(GetSkillData().ID, GetSkillData().Effect, targetStats);
                if (newEffect != null)
                {
                    targetStats.ApplyEffect(newEffect, _currentTurnID);
                }
            }

            DamageFormular.DealDamage(CalculateRawDamage(), _caster, target);
        }

        _pendingHitsCount--;
        if (_pendingHitsCount <= 0)
        {
            _skillEnd.TrySetResult();
        }
    }
}

public class TorandoData : SkillData
{
    public Vector3 Offset = new Vector3(-3, 0, 0);

    public string torandoReference = "Torando";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new TorandoSkill(owner, this);
}
