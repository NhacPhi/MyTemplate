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
    public virtual bool IsUnique => true; // Data.Unique;
    public virtual bool IsStackable => Data.MaxStack > 1;

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
    public void Tick()
    {
        if (IsStop) return;

        turn += 1;

        OnTick();

        if (turn >= Duration)
        {
            Stop();
        }
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
    protected virtual void OnTick() { }
    protected virtual void OnStop() { }
    public virtual void AddStack() 
    {
        if (IsStackable && CurrentStack < MaxStack)
        {
            turn = 0;
        }
    }

    public virtual int RemoveStack() => 0;
    public abstract StatusEffect Clone();

    public virtual string GetID() => this.ID;
    public virtual bool Equals(StatusEffect other)
    {
        if (other == null) return false;

        return this.GetID() == other.GetID()
            && this.GetType() == other.GetType();
    }
}
