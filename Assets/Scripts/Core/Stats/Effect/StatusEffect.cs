using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : IEquatable<StatusEffect>
{
    public bool IsStop { get; protected set; }
    public StatsController Target { get; protected set; }
    protected int turn;
    public abstract EffectConfig Data { get; }
    public abstract string ID { get; }

    public virtual int CurrentStack => 1;
    public virtual int MaxStack => Data.MaxStack;
    public virtual int Duration => Data.Duration;
    public virtual bool IsUnique => Data.Unique;
    public virtual bool IsStackable => Data.IsStatackable;

    public StatusEffect(StatsController target)
    {
        this.Target = target;
    }

    internal void StartEffect()
    {
        if (!this.Target) return;

        IsStop = false;
        turn = 0;
        OnStart();
    }
    public void Update()
    {
        turn += 1;
        if(turn >= Duration)
        {
            Stop();
        }

        WhileActive();
    }
    public void Stop()
    {
        IsStop = true;
        OnStop();
    }

    public virtual void Reset()
    {
        IsStop = true;
    }
    protected virtual void OnStart() { }
    protected virtual void WhileActive() { }
    protected virtual void OnStop() { }
    public virtual void AddStack() { }

    public virtual int RemoveStack() => 0;
    public abstract StatusEffect Clone();

    public virtual string GetID() => this.ID;
    public virtual bool Equals(StatusEffect other)
    {
        return this.GetID() == other.GetID()
            && this.GetType() == other.GetType();
    }
}
