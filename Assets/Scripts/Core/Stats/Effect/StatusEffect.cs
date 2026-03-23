using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : IEquatable<StatusEffect>
{
    public bool IsStop { get; protected set; }
    public StatsController Target { get; protected set; }

    protected int turn;
    public int Turn => turn;
    public abstract EffectConfig Data { get; }
    public abstract string ID { get; }

    public virtual int CurrentStack { get; protected set; } = 1;
    public virtual int MaxStack => Data.MaxStack;
    public virtual int Duration => Data.Duration;
    public virtual bool IsUnique => true; // Data.Unique;
    public virtual bool IsStackable => Data.MaxStack > 1;

    public int AppliedTurnID { get; set; }
    public StatusEffect(StatsController target)
    {
        this.Target = target;
    }

    internal void StartEffect(int currentTurnID)
    {
        if (!this.Target) return;

        IsStop = false;
        turn = 0;
        AppliedTurnID = currentTurnID;
        OnStart();
    }
    public virtual void OnStartOfTurn() { }
    public virtual void OnEndOfTurn() { }
    public virtual void ReduceDuration(int currentTurnID)
    {
        if (IsStop || Duration <= 0) return;

        if (AppliedTurnID == currentTurnID) return;

        turn += 1;

        if (Duration <= 0) return;

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

    protected virtual void OnStop() { }
    public virtual void AddStack(int currentTurnID) 
    {
        if (IsStackable && CurrentStack < MaxStack)
        {
            if (IsStackable && CurrentStack < MaxStack)
            {
                CurrentStack++;
                AppliedTurnID = currentTurnID;
                ResetDuration(); 
            }
        }
    }

    public virtual void ResetDuration()
    {
        turn = 0;
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
