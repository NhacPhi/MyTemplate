using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class CharacterStatusUI : MonoBehaviour
{
    private EntityStats entity;
    public BaseStatUI[] stats { get; private set; }

    private void Awake()
    {
        stats = GetComponentsInChildren<BaseStatUI>();
        entity = GetComponentInParent<EntityStats>();
        if (entity != null)
        {
            InitStats();
            entity.OnStatChange += HandleStatsChangeEvent;
        }
    }

    private void HandleStatsChangeEvent(StatEvtArgs stat)
    {
        var targetUI = SearchFirstStat(stat.Stat);

        if (targetUI != null)
        {
            targetUI.HandleValueChange(stat);
        }
    }

    private void InitStats()
    {
        foreach (var ui in stats)
        {
            Stat stat = entity.GetStat(ui.StatID);
            InitStat(new StatEvtArgs()
            {
                Stat = ui.StatID,
                Value = stat.Value,
            });
        }
    }

    private void InitEvent()
    {
        //Type type = typeof(PlayerStatusAction);
        //FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

        //foreach (var field in fields)
        //{
        //    Type fieldType = field.FieldType;

        //    if (fieldType == typeof(Action<AttributeEvtArgs>))
        //    {
        //        var evt = (Action<AttributeEvtArgs>)field.GetValue(null);
        //        evt += InitAttribute;
        //        field.SetValue(null, evt);
        //    }
        //    else if (fieldType == typeof(Action<StatEvtArgs>))
        //    {
        //        var evt = (Action<StatEvtArgs>)field.GetValue(null);
        //        evt += InitStat;
        //        field.SetValue(null, evt);
        //    }
        //}
    }

    private void InitAttribute(AttributeEvtArgs args)
    {
        var baseAttributeUI = SearchFirstAttribute(args.Attribute);

        if (!baseAttributeUI) return;

        baseAttributeUI.Init(args.Value, args.MaxValue);
    }

    private void InitStat(StatEvtArgs args)
    {
        var baseStatUI = SearchFirstStat(args.Stat);

        if (!baseStatUI) return;

        baseStatUI.Init(args.Value);
    }

    private BaseStatUI SearchFirstStat(StatType statType)
    {
        foreach (var stat in stats)
        {
            if (stat.StatID != statType) continue;

            return stat;
        }

        return null;
    }

    private BaseAttributeUI SearchFirstAttribute(AttributeType attributeType)
    {
        //foreach (var attribute in attributes)
        //{
        //    if (attribute.AttributeID != attributeType) continue;

        //    return attribute;
        //}

        return null;
    }
}
