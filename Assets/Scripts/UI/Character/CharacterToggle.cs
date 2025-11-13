using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterToggle : ToggleBase
{
    [SerializeField] private CharacterTap type;
    public CharacterTap Type => type;

    private void OnEnable()
    {
        if(type == CharacterTap.Info) {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }    
    }
    public void Setup(ToggleGroup group, CharacterTap type)
    {
        toggle.group = group;
        this.type = type;
    }

    public override void OnSelected(bool isOn)
    {
        if(isOn)
        {
            UIEvent.OnSelectToggleCharacterTap?.Invoke(type);
            if (type != CharacterTap.Relic)
            {
                UIEvent.OnCloseCharacterWeapon?.Invoke(true);
                UIEvent.OnSlectectRelicTap?.Invoke(false);
            }
            else
            {
                UIEvent.OnCloseCharacterWeapon?.Invoke(false);
                UIEvent.OnSlectectRelicTap?.Invoke(true);
            }
        }
    }

    public void ActiveToggle()
    {
        toggle.isOn = true;
    }
}

public enum CharacterTap
{
    None,
    Info,
    Cultivate,
    Ascend,
    Armor,
    Relic
}
