using Tech.Logger;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;
using Unity.VisualScripting;

public class AnimatorSystem : AnimationSystemBase
{
    private Animator animator;
    private int _currentTargetHash;
    protected List<(float normalizedTime, Action callback)> callbacks = new List<(float normalizedTime, Action callback)>();
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
        if (animator == null || string.IsNullOrEmpty(data.AnimationName)) return;

        callbacks.Clear();

        _currentTargetHash = data.Hash;
        animator.speed = data.TimeScale;
        animator.CrossFade(_currentTargetHash, data.Transition, data.Layer);
    }

    private void Update()
    {
        if (animator == null || callbacks.Count == 0 || animator.IsInTransition(0)) return;

        // Get information for the current layer (default is layer 0)
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash != _currentTargetHash) return;
        // Normalize time to a 0-1 range, even if the animation is looping
        float currentNormalizedTime = stateInfo.normalizedTime % 1f;

        // Iterate backwards through the list to allow for safe removal
        for (int i = callbacks.Count - 1; i >= 0; i--)
        {
            if (i >= callbacks.Count) continue;

            var tuple = callbacks[i];

            // Check if the current animation time has passed the registered timestamp
            if (currentNormalizedTime >= tuple.normalizedTime)
            {
                var action = tuple.callback;
                callbacks.RemoveAt(i);
                action?.Invoke();
            }
        }
    }

    public override void RegisterEventAtTime(float normalizedTime, Action onEventTriggered)
    {
        if (animator == null) return;
        normalizedTime = Mathf.Clamp01(normalizedTime);
        callbacks.Add((normalizedTime, onEventTriggered));
    }

}
