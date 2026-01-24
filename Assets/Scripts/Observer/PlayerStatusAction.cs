using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class PlayerStatusAction
{
    //Stat
    public static Action<Stat> OnHpChange;
    public static Action<Stat> OnAtkChange;
    public static Action<Stat> OnDefChange;

    //Atribute
    public static Action<Attribute> OnExpChange;
    public static Action<Attribute> OnLvChange;

    //Init
    public static Action<StatEvtArgs> OnInitHp;
    public static Action<StatEvtArgs> OnInitAtk;
    public static Action<StatEvtArgs> OnInitDef;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetEvents()
    {
        //C# Reflection 
        FieldInfo[] fields = typeof(PlayerStatusAction).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            if (typeof(Delegate).IsAssignableFrom(field.FieldType))
            {
                field.SetValue(null, null);
            }
        }
    }
#endif
}

public struct AttributeEvtArgs
{
    public AttributeType Attribute;
    public float Value;
    public float MaxValue;
}

public struct StatEvtArgs
{
    public StatType Stat;
    public float Value;
}
