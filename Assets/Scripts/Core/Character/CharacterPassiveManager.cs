using System.Collections.Generic;
using UnityEngine;

public class CharacterPassiveManager
{
    private CharacterProfileModel _owner;
    private List<PassiveInstance> _activePassives = new List<PassiveInstance>();
    public IReadOnlyList<PassiveInstance> Passives => _activePassives;

    public void Init(CharacterProfileModel owner) 
    { 
        _owner = owner; 
    }

    public void AddPassive(PassiveConfig config, int level)
    {
        if (config == null) return;
        
        // Dùng Factory để tạo instance
        var instance = PassiveFactory.CreatePassive(config, level, _owner);
        instance.Activate();
        _activePassives.Add(instance);
    }

    public void RemovePassive(PassiveConfig config)
    {
        var instance = _activePassives.Find(p => p.Config == config);
        if (instance != null)
        {
            instance.Dispose();
            _activePassives.Remove(instance);
        }
    }
    
    /// <summary>
    /// Lấy tổng các modifier tĩnh (+ điểm số thẳng) từ tất cả passive đang hoạt động
    /// </summary>
    public float GetTotalConstantBonus(StatType statType)
    {
        float total = 0f;
        foreach (var passive in _activePassives)
        {
            foreach (var mod in passive.Modifiers)
            {
                if (mod.Type == statType && mod.ModifierType == ModifyType.Constant)
                {
                    total += mod.TotalValue;
                }
            }
        }
        return total;
    }

    /// <summary>
    /// Lấy tổng các modifier phần trăm (+ %) từ tất cả passive đang hoạt động
    /// </summary>
    public float GetTotalPercentBonus(StatType statType)
    {
        float total = 0f;
        foreach (var passive in _activePassives)
        {
            foreach (var mod in passive.Modifiers)
            {
                if (mod.Type == statType && mod.ModifierType == ModifyType.Percent)
                {
                    total += mod.TotalValue;
                }
            }
        }
        return total;
    }
}
