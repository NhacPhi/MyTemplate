using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handler xử lý đòn Truy Kích (Follow-Up Attack) sau khi tung đòn tấn công đơn mục tiêu.
/// Thực thi đòn đánh thường (Base Skill) chính thống của nhân vật để bồi thêm sát thương vào kẻ địch còn sống.
/// Đưa hành động vào ActionQueue của BattleManager để thực thi mượt mà và an toàn tuyệt đối.
/// </summary>
public class FollowUpAttackEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        if (context == null || context.Source == null) return;

        // Chống vòng lặp truy kích và chặn kích hoạt trên kỹ năng diện rộng (AOE)
        if (context.Tags != null && (context.Tags.Contains("FollowUp") || context.Tags.Contains("CounterAttack") || context.Tags.Contains("AOE") || context.Tags.Contains("AllEnemies") || context.Tags.Contains("MultiTarget"))) return;

        var source = context.Source;
        if (source != null && source.Targets != null && source.Targets.Count > 1) return;

        var sourceStats = source != null ? source.GetCoreComponent<EntityStats>() : null;
        if (sourceStats == null || sourceStats.IsDead) return;

        // Tìm mục tiêu kẻ địch bị truy kích (ưu tiên context.Target, nếu không thì lấy source.Target)
        Entity enemyTarget = context.Target;
        if (enemyTarget == null && source.Target != null)
        {
            enemyTarget = source.Target.GetComponent<Entity>();
        }

        if (enemyTarget == null)
        {
            Debug.LogWarning($"[FollowUpAttack] Không tìm thấy enemyTarget để truy kích từ Caster {source.name}!");
            return;
        }

        var enemyStats = enemyTarget.GetCoreComponent<EntityStats>();
        if (enemyStats == null || enemyStats.IsDead)
        {
            Debug.Log($"[FollowUpAttack] Mục tiêu {enemyTarget.name} đã chết, hủy đòn Truy Kích.");
            return;
        }

        // 1. Kiểm tra xác suất kích hoạt truy kích (effectValue là %, ví dụ: 50 -> 50% cơ hội, 100 -> 100%)
        float chance = effectValue > 0 ? effectValue : 100f;
        if (chance < 100f)
        {
            float roll = Random.Range(0f, 100f);
            if (roll >= chance)
            {
                Debug.Log($"[FollowUpAttack] Xác suất {chance}% chưa kích hoạt đòn Truy Kích (Roll: {roll:F1}).");
                return; // Không kích hoạt truy kích
            }
        }

        Debug.Log($"[FollowUpAttack] Kích hoạt THÀNH CÔNG đòn Truy Kích! Caster: {source.name} -> Target: {enemyTarget.name}");

        // 2. Đưa hành động Truy Kích vào ActionQueue của BattleManager
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EnqueueAction(async () =>
            {
                if (source == null || enemyTarget == null) return;
                var sStats = source.GetCoreComponent<EntityStats>();
                var tStats = enemyTarget.GetCoreComponent<EntityStats>();
                if (sStats == null || sStats.IsDead || tStats == null || tStats.IsDead) return;

                // Gán lại mục tiêu cho Caster
                source.SetTarget(enemyTarget);
                source.HandleTurn(enemyTarget);

                // Hiển thị chữ [Truy Kích] màu cam rực rỡ trên đầu nhân vật
                string followUpText = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetLocalizedValue("STR_PURSUIT_ATTACK") 
                    : "Truy Kích";
                if (string.IsNullOrEmpty(followUpText) || followUpText == "STR_PURSUIT_ATTACK")
                {
                    followUpText = "Truy Kích";
                }

                UIEvent.TextPopup?.Invoke(followUpText, source.transform.position + Vector3.up * 1.5f, new Color(1f, 0.45f, 0.1f));

                await UniTask.Delay(150, cancellationToken: source.transform.GetCancellationTokenOnDestroy());

                var state = source.GetCoreComponent<EntityStateData>();
                if (state != null)
                {
                    state.CurrentTarget = enemyTarget;
                    state.HandleTurn();

                    source.StateManager.ChangeState(EntityState.MOVE_UP);
                    await state.WaitForMoveEnd();

                    source.StateManager.ChangeState(EntityState.ATTACK);
                    
                    var skillComp = source.GetCoreComponent<EntitySkill>();
                    var baseSkill = skillComp != null ? skillComp.GetSkill(SkillCharacter.Base) : null;
                    string sound = baseSkill != null && baseSkill.GetSkillData() != null ? baseSkill.GetSkillData().Sound : null;
                    if (!string.IsNullOrEmpty(sound))
                    {
                        source.PlaySFX(sound);
                    }

                    await state.WaitForHitFrame();

                    // Đòn truy kích: đúng chuẩn 60% ATK (0.6f)
                    var followUpDamage = new DamageBonus()
                    {
                        DamageMultiplier = 0.6f,
                        Tags = new HashSet<string> { "BasicAttack", "FollowUp", "PursuitAttack" }
                    };

                    DamageFormular.DealDamage(followUpDamage, source, enemyTarget);

                    await state.WaitForAnimEnd();

                    source.StateManager.ChangeState(EntityState.MOVE_DOWN);
                    await state.WaitForMoveEnd();
                }
                else
                {
                    var followUpDamage = new DamageBonus()
                    {
                        DamageMultiplier = 0.6f,
                        Tags = new HashSet<string> { "BasicAttack", "FollowUp", "PursuitAttack" }
                    };
                    DamageFormular.DealDamage(followUpDamage, source, enemyTarget);
                }
            });
        }
    }
}
