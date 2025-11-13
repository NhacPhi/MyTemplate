using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponToggle : ToggleBase
{
    [SerializeField] private WeaponTap type;
    public WeaponTap Type => type;

    private void OnEnable()
    {
        if (type == WeaponTap.Upgarde)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }
    }

    public override void OnSelected(bool isOn)
    {
        if(isOn)
        {
            UIEvent.OnSelectToggleWeaponTap?.Invoke(type);
        }
    }
}

public enum WeaponTap
{
    Upgarde,
    Ascend
}