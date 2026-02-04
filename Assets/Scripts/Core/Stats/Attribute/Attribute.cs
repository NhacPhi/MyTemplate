using System;
using UnityEngine;


[Serializable]
public class Attribute
{
    private float _value;
    private float _minValue;
    private Stat _maxValue;

    private AttributeType _type;
    public float Minvalue => _minValue;
    public float MaxValue => _maxValue != null ? _maxValue.Value : Utility.MAX_STAT_VALUE;

    public Action<Attribute> OnValueChange;

    public float Value
    {
        get => _value;
        set
        {
            var newValue = Math.Clamp(value, _minValue, MaxValue);
            if (Mathf.Approximately(_value, newValue)) return;

            _value = newValue;
            OnValueChange?.Invoke(this);
        }
    }

    public AttributeType Type => _type;

    public Attribute(float minValue, Stat maxValue, float startPercent, StatsController controller)
    {
        _minValue = minValue;
        _maxValue = maxValue;


        if (_maxValue != null)
        {
            _maxValue.OnValueChange += HandleMaxStatChanged;
        }

        InitValue(startPercent);
    }

    private void InitValue(float startPercent)
    {
        if (_maxValue != null)
            _value = Mathf.Lerp(_minValue, _maxValue.Value, startPercent);
        else
            _value = _minValue;
    }


    private void HandleMaxStatChanged(Stat stat)
    {
        Value = _value; 
    }

    public void Reset(float startPercent)
    {
        InitValue(startPercent);
        OnValueChange?.Invoke(this);
    }

    public void SetValueWithoutNotify(float value)
    {
        _value = Math.Clamp(value, _minValue, MaxValue);
    }


    public void Dispose()
    {
        if (_maxValue != null)
            _maxValue.OnValueChange -= HandleMaxStatChanged;
    }
}
