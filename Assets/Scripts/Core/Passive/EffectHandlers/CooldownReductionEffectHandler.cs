using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handler giảm thời gian hồi chiêu kỹ năng (vd: Tam Tiêm Lưỡng Nhận Đao psv_triple_edged_blade).
/// Khi thỏa mãn điều kiện, có effectValue% xác suất giảm hồi chiêu 1 lượt.
/// Giới hạn tối đa: Chỉ kích hoạt -1 hồi chiêu duy nhất 1 lần trong 1 lượt (Turn) của nhân vật, ngay cả khi chiêu thức đánh nhiều hit hoặc giết nhiều quái.
/// </summary>
public class CooldownReductionEffectHandler : IEffectHandler
{
    private static readonly Dictionary<int, int> _lastTriggeredTurn = new Dictionary<int, int>();

    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var recipient = target;
        if (recipient == null && context != null) recipient = context.Source;
        if (recipient == null) return;

        int currentTurn = BattleManager.Instance != null ? BattleManager.Instance.GlobalTurnID : -1;
        int entityId = recipient.GetInstanceID();

        // Kiểm tra giới hạn: Trong cùng 1 lượt, mỗi nhân vật chỉ được giảm hồi chiêu tối đa 1 lần (-1 CD)
        if (currentTurn >= 0 && _lastTriggeredTurn.TryGetValue(entityId, out int lastTurn) && lastTurn == currentTurn)
        {
            return;
        }

        // effectValue đại diện cho % xác suất kích hoạt (ví dụ: 50% -> 100%)
        float roll = Random.Range(0f, 100f);
        if (roll <= effectValue)
        {
            var skillManager = recipient.GetCoreComponent<EntitySkill>();
            if (skillManager != null)
            {
                if (currentTurn >= 0)
                {
                    _lastTriggeredTurn[entityId] = currentTurn;
                }

                skillManager.ReduceAllCooldowns(1);
                
                string cdText = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetLocalizedValue("STR_COOLDOWN_REDUCED") 
                    : "-1 Hồi Chiêu";

                if (string.IsNullOrEmpty(cdText) || cdText == "STR_COOLDOWN_REDUCED")
                {
                    cdText = "-1 Hồi Chiêu";
                }

                UIEvent.TextPopup?.Invoke(cdText, recipient.transform.position + Vector3.up * 1.5f, new Color(0.35f, 0.85f, 1f));
                UIEvent.OnUpdateSkillCharacterUI?.Invoke(recipient);
                Debug.Log($"[Passive] Tam Tiêm Đao kích hoạt thành công: Giảm 1 lượt hồi chiêu cho {recipient.name} trong lượt {currentTurn}!");
            }
        }
    }
}
