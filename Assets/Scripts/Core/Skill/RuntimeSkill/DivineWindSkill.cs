using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DivineWindSkill : SkillRuntime, IAttackSkill, IAsyncInitializer, IImpactSkill
{
    private DivineWindData skillData;
    private GameObject divineWindPrefab;
    private List<GameObject> activeTornadoClones = new List<GameObject>();

    private Entity _caster;
    private UniTaskCompletionSource _skillEnd;
    private int _pendingHitsCount = 0;

    public DivineWindSkill(EntityStats owner, DivineWindData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
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

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);
        caster.PlaySFX(skillData.Sound);
        if (state != null)
        {
            await state.WaitForAnimEnd();
        }
        caster.StateManager.ChangeState(EntityState.IDLE);

        // 1. Tìm danh sách 3 kẻ địch theo quy luật hàng cột (Column/Row Hierarchy)
        List<Entity> chosenTargets = GetThreeTargetsByColumnLogic(caster, mainTarget);
        if (chosenTargets.Count == 0 && mainTarget != null)
        {
            chosenTargets.Add(mainTarget);
        }

        _pendingHitsCount = chosenTargets.Count;

        // 2. Tạo 3 cơn lốc xoáy (VFX) phóng tới từng kẻ địch đồng thời
        for (int i = 0; i < chosenTargets.Count; i++)
        {
            var target = chosenTargets[i];
            if (target == null) continue;

            GameObject tornado = null;
            if (i == 0)
            {
                tornado = divineWindPrefab;
            }
            else
            {
                tornado = Object.Instantiate(divineWindPrefab, caster.transform.position, divineWindPrefab.transform.rotation);
                activeTornadoClones.Add(tornado);
            }

            tornado.transform.SetParent(caster.transform);
            // Độ lệch vị trí nhỏ giữa các lốc xuất phát để tạo cảm giác bão cát bùng nổ
            Vector3 offset = skillData.Offset + new Vector3(0f, (i - 1) * 0.45f, (i - 1) * 0.35f);
            tornado.transform.localPosition = offset;
            tornado.transform.localScale = _originalScale;
            tornado.gameObject.SetActive(true);

            var controller = tornado.GetComponent<DivineWindController>();
            if (controller != null)
            {
                Vector3 flyDir = target.transform.position - tornado.transform.position;
                controller.Initialize(caster, this, flyDir, target);
            }
        }

        await _skillEnd.Task;
        await UniTask.Delay(800, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

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

        // Lấy mục tiêu ưu tiên theo quy luật Hàng - Cột (Hàng trước trước, Hàng sau sau)
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
            divineWindPrefab = Object.Instantiate(ring, Vector3.zero, ring.transform.rotation);
            divineWindPrefab.gameObject.SetActive(false);
        }
    }

    public void OnDealDamage(ref float damageInput)
    {
    }

    public void OnProjectileImpact(Entity target, Vector2 contactPoint)
    {
        HandleSingleTargetDoubleDamageAsync(target).Forget();
    }

    private async UniTask HandleSingleTargetDoubleDamageAsync(Entity target)
    {
        if (target == null || _caster == null)
        {
            CheckAllHitsCompleted();
            return;
        }

        _caster.PlaySFX(skillData.SFXID);

        // 1. Đòn 1: 50% Sát thương cuồng phong
        DamageBonus hit1Bonus = CalculateRawDamage();
        hit1Bonus.DamageMultiplier *= 0.5f;

        if (target.gameObject.activeInHierarchy && !target.GetCoreComponent<EntityStats>().IsDead)
        {
            DamageFormular.DealDamage(hit1Bonus, _caster, target);
        }

        await UniTask.Delay(400, delayTiming: PlayerLoopTiming.Update,
            cancellationToken: _caster.transform.GetCancellationTokenOnDestroy());

        // 2. Đòn 2: 50% Sát thương bão xoáy và LUÔN NỔ BẠO KÍCH (100% Guaranteed Crit)
        DamageBonus hit2Bonus = CalculateRawDamage();
        hit2Bonus.DamageMultiplier *= 0.5f;
        hit2Bonus.CritRateBonus += 100f;

        if (target != null && target.gameObject.activeInHierarchy && !target.GetCoreComponent<EntityStats>().IsDead)
        {
            DamageFormular.DealDamage(hit2Bonus, _caster, target);
        }

        CheckAllHitsCompleted();
    }

    private void CheckAllHitsCompleted()
    {
        _pendingHitsCount--;
        if (_pendingHitsCount <= 0)
        {
            _skillEnd.TrySetResult();
        }
    }
}

public class DivineWindData : SkillData
{
    public Vector3 Offset = new Vector3(-4, 0, 0);

    public string torandoReference = "Divine_Wind";

    public string SFXID = "Wind_Impact";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new DivineWindSkill(owner, this);
}
