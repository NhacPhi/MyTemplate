using System;
using Tech.Composite;
using UnityEngine;

public abstract class AnimationSystemBase : CoreComponent
{
    public abstract void Play(AnimationData data);

    public abstract void RegisterEventAtTime(float normalizedTime, Action onEventTriggered);
}

public class AnimationData
{
    public string ParameterName;

    public AnimatorParameterType ParameterType;

    public object Value;

    public int Hash => Animator.StringToHash(ParameterName);

    public AnimationData Renew()
    {
        ParameterName = string.Empty;
        ParameterType = AnimatorParameterType.Trigger;
        Value = null;

        return this;
    }
}

public enum AnimatorParameterType
{
    Trigger,
    Bool,
    Int,
    Float
}
