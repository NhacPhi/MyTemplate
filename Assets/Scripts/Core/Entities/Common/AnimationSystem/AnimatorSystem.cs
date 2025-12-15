using Tech.Logger;
using System;
using UnityEngine;


public class AnimatorSystem : AnimationSystemBase
{
    private Animator animator;
    public override void LoadComponent()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            LogCommon.LogError("Animator component missing.");
        }
    }

    public override void Play(AnimationData data)
    {
        if (animator == null || string.IsNullOrEmpty(data.ParameterName)) return;

        int hash = data.Hash;
        switch (data.ParameterType)
        {
            case AnimatorParameterType.Trigger:
                animator.SetTrigger(data.ParameterName);
                break;
            case AnimatorParameterType.Bool:
                if (data.Value is bool boolValue)
                {
                    animator.SetBool(data.ParameterName, boolValue);
                }
                break;
            case AnimatorParameterType.Int:
                if (data.Value is int intValue)
                {
                    animator.SetInteger(hash, intValue);
                }
                break;
            case AnimatorParameterType.Float:
                if (data.Value is float floatValue)
                {
                    animator.SetFloat(hash, floatValue);
                }
                break;
            default:
                LogCommon.LogWarning($"Unsupported Animator Parameter Type: {data.ParameterType}");
                break;
        }
    }

    public override void RegisterEventAtTime(float normalizedTime, Action onEventTriggered)
    {
        if (animator == null) return;


    }

}
