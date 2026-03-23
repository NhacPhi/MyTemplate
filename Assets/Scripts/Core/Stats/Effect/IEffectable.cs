using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectable
{
    public void ApplyEffect(StatusEffect effect, int currentTurnID);
    public void RemoveEffect(StatusEffect effect, bool ignoreStack);
    public bool HasEffect<T>() where T : StatusEffect;
}