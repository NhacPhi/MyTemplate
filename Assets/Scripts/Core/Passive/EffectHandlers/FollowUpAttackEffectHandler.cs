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

        // Chống vòng lặp truy kích vô tận (chỉ kích hoạt từ đòn đánh chủ động, không kích hoạt từ phản đòn hoặc chính đòn truy kích)
        if (context.Tags != null && (context.Tags.Contains("FollowUp") || context.Tags.Contains("CounterAttack"))) return;

        var source = context.Source;
        var sourceStats = source.GetCoreComponent<EntityStats>();
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

                // Hiển thị chữ [Truy Kích] màu cam rực rỡ trên đầu nhân vật
                string followUpText = LocalizationManager.Instance.GetLocalizedValue("STR_PURSUIT_ATTACK");
                if (string.IsNullOrEmpty(followUpText) || followUpText == "STR_PURSUIT_ATTACK")
                {
                    followUpText = "Truy Kích";
                }

                UIEvent.TextPopup?.Invoke(followUpText, source.transform.position + Vector3.up * 1.5f, new Color(1f, 0.45f, 0.1f));

                // Chờ ngắn để hiển thị hiệu ứng Text Popup
                await UniTask.Delay(250);

                // Lấy kỹ năng đánh thường (BaseSkill) của nhân vật để thực thi
                var skillComp = source.GetCoreComponent<EntitySkill>();
                var baseSkill = skillComp != null ? skillComp.GetSkill(SkillCharacter.Base) : null;

                if (baseSkill != null)
                {
                    int turnId = BattleManager.Instance != null ? BattleManager.Instance.GlobalTurnID : 0;
                    await baseSkill.ExecuteAsync(source, turnId);
                }
                else
                {
                    // Fallback an toàn nếu nhân vật không có BaseSkill
                    DamageBonus bonus = new DamageBonus
                    {
                        DamageMultiplier = 1.0f,
                        CritRateBonus = 0,
                        CritDmgBonus = 0,
                        PenetrationBonus = 0
                    };
                    bonus.AddTag("FollowUp");
                    DamageFormular.DealDamage(bonus, source, enemyTarget);
                }
            });
        }
    }
}
