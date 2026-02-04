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
    public BaseAttributeUI[] attributes { get; private set; }

    private void Awake()
    {
        attributes = GetComponentsInChildren<BaseAttributeUI>();

        entity = GetComponentInParent<EntityStats>();

        if (entity != null)
        {
            InitAttributes();
            entity.OnAtributeChange += HandleAttributesChangeEvent;
        }
    }

    private void HandleAttributesChangeEvent(AttributeEvtArgs attribute)
    {
        var targetUI = SearchFirstAttribute(AttributeType.Hp);

        if (targetUI != null)
        {
            targetUI.HandleValueChange(attribute);
        }
    }

    private void InitAttributes()
    {
        foreach (var ui in attributes)
        {
            Attribute attribute = entity.GetAttribute(ui.AttributeID);
            InitAttribute(new AttributeEvtArgs()
            { 
                Attribute = AttributeType.Hp, /// Hard code
                Value = attribute.Value,
                MaxValue = attribute.MaxValue
            });
        }
    }

    private void InitEvent()
    {
       
    }

    private void InitAttribute(AttributeEvtArgs args)
    {
        var baseAttributeUI = SearchFirstAttribute(args.Attribute);

        if (!baseAttributeUI) return;

        baseAttributeUI.Init(args.Value, args.MaxValue);
    }


    private BaseAttributeUI SearchFirstAttribute(AttributeType type)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeID != type) continue;

            return attribute;
        }

        return null;
    }
}
