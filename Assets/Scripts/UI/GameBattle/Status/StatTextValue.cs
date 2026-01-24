using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatTextValue : BaseStatUI
{
    [SerializeField] protected TextMeshProUGUI textValue;
    protected virtual void Reset()
    {
        var allTransform = GetComponentsInChildren<Transform>(true);
        OnReset(allTransform);
    }

    protected virtual void OnReset(Transform[] allTransform)
    {
        if (!textValue)
        {
            textValue = allTransform.FirstOrDefault(x => x.name.ToLower()
                .Contains("txtvalue")).GetComponentInChildren<TextMeshProUGUI>();
        }

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            if (!type.ToString().ToLower().Contains(this.name.ToLower())) continue;

            this.StatID = type;
            break;
        }
    }

    public override void Init(float value)
    {
        textValue.text = Mathf.CeilToInt(value).ToString(CultureInfo.InvariantCulture);
    }

    public override void HandleValueChange(StatEvtArgs stat)
    {
        //textValue.text = Mathf.CeilToInt(stat.Value).ToString(CultureInfo.InvariantCulture);
    }
}
