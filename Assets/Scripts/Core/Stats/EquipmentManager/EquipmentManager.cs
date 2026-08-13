using System.Collections.Generic;
using System.Linq;

public class EquipmentManager 
{
    private Dictionary<EquipSlot, EquipmentData> _equippedItems = new Dictionary<EquipSlot, EquipmentData>();

    public IReadOnlyDictionary<EquipSlot, EquipmentData> EquippedItems => _equippedItems;

    private SetBonusEvaluator _setBonusEvaluator;

    public void Init(SetBonusEvaluator evaluator)
    {
        _setBonusEvaluator = evaluator;
    }

    public EquipmentData Equip(EquipmentData newItem)
    {
        if (newItem == null) return null;

        EquipmentData oldItem = null;

        if(_equippedItems.ContainsKey(newItem.Slot))
        {
            oldItem = _equippedItems[newItem.Slot];
        }

        _equippedItems[newItem.Slot] = newItem;

        return oldItem;
    }

    public EquipmentData Unequip(EquipSlot slot)
    {
        if(_equippedItems.TryGetValue(slot, out EquipmentData oldItem))
        {
            _equippedItems.Remove(slot);
            return oldItem;
        }

        return null;    
    }

    public float GetTotalFlatBonus(StatType statType)
    {
        float total = 0f;

        foreach(var item in _equippedItems.Values)
        {
            foreach(var mod in item.Modifiers)
            {
                if(mod.Type == statType && mod.ModifierType == ModifyType.Flat)
                {
                    total += mod.TotalValue;
                }
            }
        }

        var activeBonuses = _setBonusEvaluator.GetActiveSetBonuses(this);
        if(activeBonuses != null)
        {
            total += activeBonuses.Where(b => b.Stat == statType && b.Modifier == ModifyType.Flat)
                      .Sum(b => b.Value);
        }

        return total;
    }

    public float GetWeaponFlatBonus(StatType statType)
    {
        float total = 0f;
        if (_equippedItems.TryGetValue(EquipSlot.Weapon, out var weapon))
        {
            foreach (var mod in weapon.Modifiers)
            {
                if (mod.Type == statType && mod.ModifierType == ModifyType.Flat)
                {
                    total += mod.TotalValue;
                }
            }
        }
        return total;
    }

    public float GetArmorFlatBonus(StatType statType)
    {
        float total = 0f;
        foreach (var kvp in _equippedItems)
        {
            if (kvp.Key == EquipSlot.Weapon) continue;
            foreach (var mod in kvp.Value.Modifiers)
            {
                if (mod.Type == statType && mod.ModifierType == ModifyType.Flat)
                {
                    total += mod.TotalValue;
                }
            }
        }

        var activeBonuses = _setBonusEvaluator.GetActiveSetBonuses(this);
        if (activeBonuses != null)
        {
            total += activeBonuses.Where(b => b.Stat == statType && b.Modifier == ModifyType.Flat)
                      .Sum(b => b.Value);
        }

        return total;
    }

    public float GetTotalPercentBonus(StatType statType)
    {
        float total = 0f;

        foreach(var item in _equippedItems.Values)
        {
            foreach(var mod in item.Modifiers)
            {
                if(mod.Type == statType && mod.ModifierType == ModifyType.Percent)
                {
                    total += mod.TotalValue;    
                }
            }
        }

        var activeBonuses = _setBonusEvaluator.GetActiveSetBonuses(this);

        if(activeBonuses != null)
        {
            total += activeBonuses.Where(b => b.Stat == statType && b.Modifier == ModifyType.Percent)
                      .Sum(b => b.Value);
        }

        return total;
    }

    public List<SetBonusConfig> GetActiveSetBonuses()
    {
        if (_setBonusEvaluator == null) return new List<SetBonusConfig>();

        // Gọi Evaluator để quét đồ đang mặc và trả về list config
        return _setBonusEvaluator.GetActiveSetBonuses(this);
    }
}
