using System.Collections.Generic;
public class EquipmentManager 
{
    private Dictionary<EquipSlot, EquipmentData> _equippedItems = new Dictionary<EquipSlot, EquipmentData>();

    public IReadOnlyDictionary<EquipSlot, EquipmentData> EquippedItems => _equippedItems;

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

    public float GetTotalConstantBonus(StatType statType)
    {
        float total = 0f;

        foreach(var item in _equippedItems.Values)
        {
            foreach(var mod in item.Modifiers)
            {
                if(mod.Type == statType && mod.ModifierType == ModifyType.Constant)
                {
                    total += mod.TotalValue;
                }
            }
        }

        return total;
    }

    public float GetTotalPercenBonus(StatType statType)
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

        return total;
    }
}
