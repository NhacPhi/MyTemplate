using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAttributeUI : MonoBehaviour
{
    [field: SerializeField] public AttributeType AttributeID { get; protected set; }
    public float LastValue { get; protected set; }
    public float LastMaxValue { get; protected set; }
    private void Awake()
    {
        InitEvent();
        OnAwake();
    }
    protected virtual void OnAwake() { }
    public abstract void Init(float value, float maxValue);
    protected virtual void InitEvent()
    {
        switch (AttributeID)
        {
            case AttributeType.Hp:

                return;
            case AttributeType.Shield:

                return;
        }
    }
    public abstract void HandleValueChange(AttributeEvtArgs attribute);
    public abstract void HandleMaxValueChange(Stat stat);
}
