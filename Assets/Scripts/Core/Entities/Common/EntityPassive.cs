using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Tech.Composite;
using VContainer;

public class EntityPassive : CoreComponent, IAsyncInitializer
{
    private EntityStats _entityStats;
    private Entity _entity;
    
    [Inject] private PlayerCharacterManager _playerCharacterManager;

    // Lưu trữ các Passive đang hoạt động trong trận
    public List<PassiveInstance> ActivePassives { get; private set; } = new List<PassiveInstance>();

    public async UniTask InitializeAsync(CancellationToken token)
    {
        _entity = (core as Entity) ?? GetComponent<Entity>() ?? GetComponentInParent<Entity>();
        _entityStats = (_entity != null ? _entity.GetCoreComponent<EntityStats>() : null) ?? GetComponent<EntityStats>() ?? GetComponentInParent<EntityStats>();

        IReadOnlyList<PassiveInstance> passivesToLoad = null;

        // 1. Nếu là Enemy / Boss có EnemyProfileModel
        if (_entityStats != null && _entityStats.StatProvider is EnemyProfileModel enemyProfile && enemyProfile.PassivesManager != null)
        {
            passivesToLoad = enemyProfile.PassivesManager.Passives;
        }
        // 2. Nếu là Nhân vật của Player
        else if (_playerCharacterManager != null && _entityStats != null && !string.IsNullOrEmpty(_entityStats.EntityID))
        {
            var characterProfile = _playerCharacterManager.GetCharacter(_entityStats.EntityID);
            if (characterProfile != null && characterProfile.PassivesManager != null)
            {
                passivesToLoad = characterProfile.PassivesManager.Passives;
            }
        }
        // 3. Fallback khác
        if (passivesToLoad == null && _entityStats != null)
        {
            if (_entityStats.StatProvider is CharacterProfileModel charProfile && charProfile.PassivesManager != null)
            {
                passivesToLoad = charProfile.PassivesManager.Passives;
            }
        }

        if (passivesToLoad == null || passivesToLoad.Count == 0)
        {
            return;
        }

        UnityEngine.Debug.Log($"[EntityPassive] Entity {_entityStats?.EntityID} nạp thành công {passivesToLoad.Count} nội tại (Passives).");

        // 3. Nạp toàn bộ PassiveInstance đang được trang bị/mở khóa
        foreach (var passiveInstance in passivesToLoad)
        {
            string firstEffectId = (passiveInstance.Config?.CombatEvents != null && passiveInstance.Config.CombatEvents.Count > 0) 
                ? passiveInstance.Config.CombatEvents[0].EffectId 
                : "None";
            UnityEngine.Debug.Log($"[EntityPassive] Đang đăng ký nội tại: {firstEffectId}");
            ActivePassives.Add(passiveInstance);

            // 4. Truyền Entity vào để Passive tự động đăng ký Event
            passiveInstance.SubscribeToEntity(_entity);
        }

        await UniTask.CompletedTask;
    }

    public void ProcessDamageBonus(ref DamageBonus bonus, Entity target, HashSet<string> tags = null)
    {
        if (ActivePassives == null || ActivePassives.Count == 0) return;

        var context = new CombatContext(_entity, target, 0f);
        context.DamageBonus = bonus;

        if (tags != null)
        {
            foreach (var tag in tags) context.AddTag(tag);
        }
        if (bonus.Tags != null)
        {
            foreach (var tag in bonus.Tags) context.AddTag(tag);
        }

        foreach (var passive in ActivePassives)
        {
            if (passive.Config == null || passive.Config.CombatEvents == null) continue;

            foreach (var evtConfig in passive.Config.CombatEvents)
            {
                if (evtConfig.EventType == "OnBeforeDealDamage")
                {
                    PassiveEventListener.EvaluateAndExecute(evtConfig, passive.Level, context);
                }
            }
        }

        if (context.DamageBonus.HasValue)
        {
            bonus = context.DamageBonus.Value;
        }
    }

    private void OnDestroy()
    {
        // Gỡ sự kiện khi Entity bị hủy để tránh Memory Leak
        foreach (var passive in ActivePassives)
        {
            if (_entity != null)
            {
                passive.UnsubscribeFromEntity(_entity);
            }
        }
    }
}
