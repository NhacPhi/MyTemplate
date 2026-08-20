using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handler xử lý phản đòn (Counter-Attack) khi bị tấn công.
/// Gây sát thương dựa trên % ATK của Source lên kẻ vừa tấn công.
/// Đưa hành động vào BattleManager.ActionQueue để kẻ địch Move Down xong mới tiến hành Phản Kích.
/// </summary>
public class CounterAttackEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        if (target == null || context == null || context.Source == null) return;

        // Chống vòng lặp phản đòn vô tận
        if (context.Tags != null && context.Tags.Contains("CounterAttack")) return;

        var source = context.Source;
        var sourceStats = source.GetCoreComponent<EntityStats>();
        var targetStats = target.GetCoreComponent<EntityStats>();

        if (sourceStats == null || sourceStats.IsDead || targetStats == null || targetStats.IsDead) return;

        // 1. Kiểm tra tỷ lệ xác suất phản kích (effectValue là % cơ hội, ví dụ: 50 -> 50% cơ hội)
        float chance = effectValue > 0 ? effectValue : 100f;
        if (chance < 100f)
        {
            float roll = Random.Range(0f, 100f);
            if (roll >= chance)
            {
                return; // Không kích hoạt phản đòn
            }
        }

        // 2. Đưa hành động Phản Kích vào Hàng Đợi (ActionQueue) của BattleManager
        // Đảm bảo kẻ tấn công ban đầu hoàn tất việc đánh và Move Down về vị trí cũ trước,
        // sau đó Ngưu Ma Vương mới tiến lên Move Up -> Attack -> Gây ST Phản Đòn -> Move Down!
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EnqueueAction(async () =>
            {
                if (source == null || target == null) return;
                var sStats = source.GetCoreComponent<EntityStats>();
                var tStats = target.GetCoreComponent<EntityStats>();
                if (sStats == null || sStats.IsDead || tStats == null || tStats.IsDead) return;

                source.SetTarget(target);
                source.HandleTurn(target);

                // Hiển thị chữ [Phản Kích] trên đầu nhân vật phản đòn
                string counterText = LocalizationManager.Instance.GetLocalizedValue("STR_COUNTER_ATTACK");

                UIEvent.TextPopup?.Invoke(counterText, source.transform.position + Vector3.up * 1.5f, new Color(1f, 0.85f, 0.2f));

                var stateData = source.GetCoreComponent<EntityStateData>();
                if (stateData != null && source.StateManager != null)
                {
                    // Move Up đến trước mặt kẻ địch
                    source.StateManager.ChangeState(EntityState.MOVE_UP);
                    await stateData.WaitForMoveEnd();

                    // Vung đao tấn công
                    source.StateManager.ChangeState(EntityState.ATTACK);
                    var skillComp = source.GetCoreComponent<EntitySkill>();
                    var baseSkill = skillComp != null ? skillComp.GetSkill(SkillCharacter.Base) : null;
                    if (baseSkill != null && baseSkill.GetSkillData() != null)
                    {
                        source.PlaySFX(baseSkill.GetSkillData().Sound);
                    }

                    await stateData.WaitForHitFrame();

                    // Gây sát thương phản đòn
                    DamageBonus bonus = new DamageBonus
                    {
                        DamageMultiplier = 1.0f,
                        CritRateBonus = 0,
                        CritDmgBonus = 0,
                        PenetrationBonus = 0
                    };
                    bonus.AddTag("CounterAttack");

                    DamageFormular.DealDamage(bonus, source, target);

                    await stateData.WaitForAnimEnd();

                    // Move Down lùi về vị trí cũ
                    source.StateManager.ChangeState(EntityState.MOVE_DOWN);
                    await stateData.WaitForMoveEnd();
                }
                else
                {
                    DamageBonus bonus = new DamageBonus
                    {
                        DamageMultiplier = 1.0f,
                        CritRateBonus = 0,
                        CritDmgBonus = 0,
                        PenetrationBonus = 0
                    };
                    bonus.AddTag("CounterAttack");
                    DamageFormular.DealDamage(bonus, source, target);
                }
            });
        }
        else
        {
            DamageBonus bonus = new DamageBonus
            {
                DamageMultiplier = 1.0f,
                CritRateBonus = 0,
                CritDmgBonus = 0,
                PenetrationBonus = 0
            };
            bonus.AddTag("CounterAttack");
            DamageFormular.DealDamage(bonus, source, target);
        }
    }
}
