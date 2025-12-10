using System;
using System.Collections.Generic;


public enum ModifyType
{
    BaseConstant,
    Constant,
    Percent,
}

public class Modifier : IEquatable<Modifier>, IEqualityComparer<Modifier>
{
    public float Value;
    public ModifyType Type;

   public Modifier(float value, ModifyType type)
    {
        Value = value;
        Type = type;
    }

    public bool Equals(Modifier other)
    {
        return Value.Equals(other.Value) && Type == other.Type;
    }

    public bool Equals(Modifier x, Modifier y)
    {
        return x.Value.Equals(y.Value) && x.Type == y.Type;
    }

    public int GetHashCode(Modifier obj)
    {
        return HashCode.Combine(Value, obj.Value);
    }

}
