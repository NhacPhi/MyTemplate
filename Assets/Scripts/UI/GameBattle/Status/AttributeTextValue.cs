using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class AttributeTextValue : BaseAttributeUI
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
                .Contains("textvalue")).GetComponentInChildren<TextMeshProUGUI>();
        }

        foreach (AttributeType type in Enum.GetValues(typeof(AttributeType)))
        {
            if (!type.ToString().ToLower().Contains(this.name.ToLower())) continue;

            this.AttributeID = type;
            break;
        }
    }

    //CultureInfo.InvariantCulture result string ignore culture 76.54 => 76,54 if France
    public override void Init(float value, float maxValue)
    {
        LastValue = value;
        LastMaxValue = maxValue;
        SetTextValue();
    }

    public override void HandleValueChange(AttributeEvtArgs attribute)
    {
        LastValue = attribute.Value;
        LastMaxValue = attribute.MaxValue;
        SetTextValue();
    }

    public override void HandleMaxValueChange(Stat stat)
    {
        LastMaxValue = stat.Value;
        SetTextValue();
    }

    protected virtual void SetTextValue()
    {
        textValue.text = Mathf.CeilToInt(this.LastValue).ToString();
    }
}
