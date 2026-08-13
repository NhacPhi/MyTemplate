using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Stat
{
    private float baseValue;

    public float BaseValue => baseValue;

    private bool isDirty = true;
    private float value;
    public float Value
    {
        get
        {
            if(isDirty)
            {
                ReCalculateValue();
                isDirty = false;
            }

            return value;
        }
    }

    public Action<Stat> OnValueChange;

    private List<Modifier> statModifiers;
    public int ModifierCount => statModifiers.Count;
    public Stat()
    {
        statModifiers = new List<Modifier>();
    }

    public Stat(float baseValue) : this()
    {
        this.baseValue = baseValue;
    }
    public void AddModifierWithoutNotify(Modifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
    }


    public void AddModifier(Modifier mod)
    {
        statModifiers.Add(mod);
        ReCalculateValue();
        OnValueChange?.Invoke(this);
    }

    public bool RemoveModifier(Modifier mod)
    {
        if (!statModifiers.Remove(mod)) return false;

        ReCalculateValue();
        OnValueChange?.Invoke(this);
        return true;
    }

    public bool RemoveModifierWithoutNotify(Modifier mod)
    {
        if (statModifiers.Remove(mod))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    public void ClearAllModifiers()
    {
        statModifiers.Clear();
        value = baseValue;
        OnValueChange?.Invoke(this);
    }


    public virtual void ReCalculateValue()
    {
        float baseFlat = 0f;
        float flat = 0f;
        float percent = 0f;

        if(statModifiers!= null)
        {
            foreach (Modifier modifier in statModifiers)
            {
                switch (modifier.Type)
                {
                    case ModifyType.BaseFlat:
                        baseFlat += modifier.Value;
                        continue;
                    case ModifyType.Flat:
                        flat += modifier.Value;
                        continue;
                    case ModifyType.Percent:
                        percent += modifier.Value;
                        continue;
                };
            }

        }

        float filaValue = (BaseValue + baseFlat) * (1f + percent) + flat;

        value = (float)Math.Round(filaValue, 0);
    }
}
