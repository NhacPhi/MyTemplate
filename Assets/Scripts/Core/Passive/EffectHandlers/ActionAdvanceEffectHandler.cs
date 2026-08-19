using UnityEngine;

/// <summary>
/// Handler tăng Điểm Hành Động (Action Point / Action Advance) cho nhân vật.
/// Giảm CurrentAV trong TurnOrderSystem để đẩy nhanh lượt đánh tiếp theo.
/// </summary>
public class ActionAdvanceEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var recipient = target;
        if (recipient == null && context != null) recipient = context.Source;
        if (recipient == null) return;

        if (BattleManager.Instance != null && BattleManager.Instance.TurnSystem != null)
        {
            BattleManager.Instance.TurnSystem.AdvanceAction(recipient, effectValue);
        }
    }
}
