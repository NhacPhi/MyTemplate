using System;
using UnityEngine;

[Serializable]
public class Attribute
{
    private float _value;
    private float _minValue;
    private Stat _maxValue;

    public float Minvalue => _minValue;
    public float MaxValue => _maxValue?.Value ?? 0;

    public Action<Attribute> OnValueChange;
    public float Value
    {
        get
        {
            _value = Math.Clamp(_value, _minValue, MaxValue);
            return _value;
        }

        set
        {
            var newValue = Math.Clamp(value, _minValue, MaxValue);

            if (_value.Equals(newValue)) return;

            _value = newValue;

            OnValueChange?.Invoke(this);
        }
    }


    public Attribute(float minValue, Stat maxValue, float startPercent, StatsController controller)
    {
        _minValue = minValue;
        _maxValue = maxValue;

        _value = Mathf.Lerp(minValue, MaxValue, startPercent); 
    }

    public void Reset(float startPercent)
    {
        _value = Mathf.Lerp(_minValue, MaxValue, startPercent);
        OnValueChange?.Invoke(this);
    }

    public void SetValueWithoutNotify(float value)
    {
        _value = Mathf.Clamp(_value, _minValue, MaxValue);
    }
}
