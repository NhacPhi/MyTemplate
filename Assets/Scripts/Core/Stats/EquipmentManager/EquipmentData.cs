using System.Collections.Generic;

public enum EquipSlot
{
    Weapon,
    Helmet,
    Chestplate,
    Gloves,
    Boots,
    Belt,
    Ring
}

public class EquipmentData
{
    public string UUID;
    public int Level;

    public EquipSlot Slot;
    public string SetName;

    public ItemConfig BaseConfig;
    public List<EquipModifier> Modifiers = new List<EquipModifier>();
}
