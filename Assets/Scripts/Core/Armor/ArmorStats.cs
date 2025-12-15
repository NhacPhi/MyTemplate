using System;
public class ArmorStats 
{
    private StatType type;
    private int point;
    private ModifyType modifyType;
    private int level;

    public StatType Type { get { return type; } set { type = value; } }
    public int Point { get { return point; } set { point = value; } }
    public int Level { get { return level; } set { level = value; } }
    public ModifyType ModifyType { get { return modifyType; } set { modifyType = value; } }
}
