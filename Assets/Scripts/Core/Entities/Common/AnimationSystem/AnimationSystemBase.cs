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
    public string AnimationName;

    public float Transition = 0.1f;
    public float TimeScale = 1f;
    public int Layer = 0;
    private int _hash = 0;
    public int Hash
    {
        get
        {
            if (_hash == 0 && !string.IsNullOrEmpty(AnimationName))
                _hash = Animator.StringToHash(AnimationName);
            return _hash;
        }
    }

    public AnimationData Renew()
    {
        AnimationName = string.Empty;
        _hash = 0; 
        Transition = 0.1f;
        TimeScale = 1f;
        Layer = 0;
        return this;
    }
}

