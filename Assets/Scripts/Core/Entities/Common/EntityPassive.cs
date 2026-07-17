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
        _entity = core as Entity;
        _entityStats = gameObject.GetComponent<EntityStats>();

        // 1. Lấy Data nhân vật
        var characterProfile = _playerCharacterManager.GetCharacter(_entityStats.EntityID);
        if (characterProfile == null || characterProfile.PassivesManager == null) 
        {
            UnityEngine.Debug.LogWarning($"[EntityPassive] Không tìm thấy Data nhân vật hoặc PassivesManager cho Entity: {_entityStats.EntityID}");
            return;
        }

        UnityEngine.Debug.Log($"[EntityPassive] Entity {_entityStats.EntityID} nạp thành công {characterProfile.PassivesManager.Passives.Count} nội tại.");

        // 2. Nạp toàn bộ PassiveInstance đang được trang bị/mở khóa
        foreach (var passiveInstance in characterProfile.PassivesManager.Passives)
        {
            string firstEffectId = (passiveInstance.Config?.CombatEvents != null && passiveInstance.Config.CombatEvents.Count > 0) 
                ? passiveInstance.Config.CombatEvents[0].EffectId 
                : "None";
            UnityEngine.Debug.Log($"[EntityPassive] Đang đăng ký nội tại: {firstEffectId}");
            ActivePassives.Add(passiveInstance);

            // 3. Truyền Entity vào để Passive tự động đăng ký Event
            passiveInstance.SubscribeToEntity(_entity);
        }

        await UniTask.CompletedTask;
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
