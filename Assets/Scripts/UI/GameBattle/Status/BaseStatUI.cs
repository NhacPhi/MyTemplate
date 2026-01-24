using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatUI : MonoBehaviour
{
    [field: SerializeField] public StatType StatID { get; protected set; }

    public float LastValue { get; protected set; }
    private void Awake()
    {
        InitEvent();
        OnAwake();
    }

    protected virtual void InitEvent()
    {
        //switch (StatID)
        //{
        //    case StatType.ATK:
        //        PlayerStatusAction.OnAtkChange += HandleValueChange;
        //        return;
        //    case StatType.DEF:
        //        PlayerStatusAction.OnDefChange += HandleValueChange;
        //        return;
        //    case StatType.HP:
        //        PlayerStatusAction.OnHpChange += HandleValueChange;
        //        return;
        //    default:
        //        return;
        //}
    }

    protected virtual void OnAwake() { }
    public abstract void Init(float value);
    public abstract void HandleValueChange(StatEvtArgs stat);
}
