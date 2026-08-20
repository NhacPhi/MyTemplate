using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NezhaExecutionSkill : SkillRuntime, IAttackSkill
{
    private NezhaExecutionData skillData;

    public NezhaExecutionSkill(EntityStats owner, NezhaExecutionData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override DamageBonus CalculateRawDamage()
    {
        var bonus = base.CalculateRawDamage();
        if (bonus.Tags == null) bonus.Tags = new HashSet<string>();
        bonus.Tags.Add("UltimateSkill");
        return bonus;
    }

    public void OnDealDamage(ref float damageInput)
    {
        // Handled dynamically by Event Handlers in Damage Pipeline
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var enemy = caster.Target != null ? caster.Target.gameObject.GetComponent<Entity>() : null;
        if (enemy == null) return;

        caster.HandleTurn(enemy);
        var state = caster.GetCoreComponent<EntityStateData>();

        // 1. Áp sát mục tiêu ban đầu và thi triển Tuyệt Kỹ Ultimate
        caster.StateManager.ChangeState(EntityState.MOVE_UP);
        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);
        caster.PlaySFX(skillData.Sound);

        await state.WaitForHitFrame();

        if (!enemy.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(caster, currentTurnID);
        }

        DamageFormular.DealDamage(CalculateRawDamage(), caster, enemy);

        await state.WaitForAnimEnd();

        // 2. Kiểm tra nếu KẾT LIỄU mục tiêu -> Kích hoạt hiệu ứng Càn Quét Chiến Trường
        var enemyStats = enemy.GetCoreComponent<EntityStats>();
        if (enemyStats != null && enemyStats.IsDead)
        {
            Entity nextTarget = GetRandomValidTarget(caster);

            if (nextTarget != null)
            {
                // Hiển thị text hiệu ứng Càn Quét từ Localization
                string sweepText = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetLocalizedValue("STR_SWEEP_ATTACK") 
                    : "Càn Quét";

                UIEvent.TextPopup?.Invoke(sweepText, caster.transform.position + Vector3.up * 1.5f, new Color(1f, 0.55f, 0.1f));

                await UniTask.Delay(150, cancellationToken: caster.transform.GetCancellationTokenOnDestroy());

                // Chuyển hướng và lướt sang mục tiêu kế tiếp tuân thủ luật Hàng & Cột
                caster.SetTarget(nextTarget);
                state.CurrentTarget = nextTarget;
                state.HandleTurn();

                caster.StateManager.ChangeState(EntityState.MOVE_UP);
                await state.WaitForMoveEnd();

                // Tung đòn Đánh Thường càn quét (120% ATK)
                caster.StateManager.ChangeState(EntityState.ATTACK);
                caster.PlaySFX("ThirdPrinceNezha_Attack");

                await state.WaitForHitFrame();

                var sweepDamage = new DamageBonus()
                {
                    DamageMultiplier = 1.2f,
                    Tags = new HashSet<string> { "BasicAttack", "SweepAttack", "PursuitAttack" }
                };

                DamageFormular.DealDamage(sweepDamage, caster, nextTarget);

                await state.WaitForAnimEnd();
            }
        }

        // 3. Thu chiêu trở về vị trí ban đầu
        caster.StateManager.ChangeState(EntityState.MOVE_DOWN);
        await state.WaitForMoveEnd();

        PutOnCooldown();
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

public class NezhaExecutionData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new NezhaExecutionSkill(owner, this);
}
