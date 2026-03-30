using System;

[Serializable]
public class EquipModifier
{
    public StatType Type;
    public ModifyType ModifierType;


    public float BaseValue;
    public float UpgradeBonus;

    public float TotalValue => BaseValue  + UpgradeBonus;
}
