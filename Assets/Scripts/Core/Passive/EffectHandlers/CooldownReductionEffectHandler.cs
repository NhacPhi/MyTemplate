using UnityEngine;

/// <summary>
/// Handler giảm thời gian hồi chiêu kỹ năng (vd: Tam Tiêm Lưỡng Nhận Đao psv_triple_edged_blade).
/// Khi thỏa mãn điều kiện (VD: dùng skill đơn kích sát mục tiêu), có effectValue% xác suất giảm hồi chiêu 1 lượt.
/// </summary>
public class CooldownReductionEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var recipient = target;
        if (recipient == null && context != null) recipient = context.Source;
        if (recipient == null) return;

        // effectValue đại diện cho % xác suất kích hoạt (ví dụ: 30% -> 80%)
        float roll = Random.Range(0f, 100f);
        if (roll <= effectValue)
        {
            var skillManager = recipient.GetCoreComponent<EntitySkill>();
            if (skillManager != null)
            {
                skillManager.ReduceAllCooldowns(1);
                
                string cdText = LocalizationManager.Instance != null 
                    ? LocalizationManager.Instance.GetLocalizedValue("STR_COOLDOWN_REDUCED") 
                    : "-1 Hồi Chiêu";

                if (string.IsNullOrEmpty(cdText) || cdText == "STR_COOLDOWN_REDUCED")
                {
                    cdText = "-1 Hồi Chiêu";
                }

                UIEvent.TextPopup?.Invoke(cdText, recipient.transform.position + Vector3.up * 1.5f, new Color(0.35f, 0.85f, 1f));
                Debug.Log($"[Passive] Tam Tiêm Đao kích hoạt thành công: Giảm 1 lượt hồi chiêu cho {recipient.name}!");
            }
        }
    }
}
