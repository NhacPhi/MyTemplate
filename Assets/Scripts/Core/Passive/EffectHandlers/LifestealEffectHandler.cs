using UnityEngine;

public class LifestealEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        // context.Value lưu trữ lượng sát thương vừa gây ra
        float damageDealt = context.Value;
        
        // Hồi máu dựa trên % sát thương gây ra
        float healAmount = damageDealt * (effectValue / 100f);
        
        Debug.Log($"[PassiveSystem] Lifesteal kích hoạt! Sát thương: {damageDealt}, Máu hồi: {healAmount}");
        stats.HealingHP(healAmount);
        
        // Tùy chọn: Gọi UI hiển thị máu hồi
        UIEvent.HealPopup?.Invoke(healAmount, target.transform.position);
    }
}
