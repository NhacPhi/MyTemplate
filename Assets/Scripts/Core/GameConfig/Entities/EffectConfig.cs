using System;

public class EffectConfig
{
    private string id;
    private string name;
    private string description;

    private StatusEffect effectType;
    private bool unique;
    private bool isStackable;
    private int maxStack;
    private int duration; // turn count

    public string ID { get { return id; } set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Description { get { return description; } set { description = value; } }
    public StatusEffect EffectType { get { return effectType; } set { effectType = value; } }
    public bool Unique { get { return unique; } set { unique = value; } }
    public bool IsStatackable { get { return isStackable; } set { isStackable = value; } }
    public int MaxStack { get { return maxStack; } set { maxStack = value; }  }
    public int Duration { get { return duration; } set { duration = value; } }
}
