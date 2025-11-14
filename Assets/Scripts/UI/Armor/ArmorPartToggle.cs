using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPartToggle : ToggleBase
{
    [SerializeField] private ArmorPart part;
    public ArmorPart Part => part;

    public override void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnUpdateCharacterCategoryArmor?.Invoke(part);
        }
    }
}
