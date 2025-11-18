using System;
public class ArmorStats 
{
    private StatsPool type;
    private int point;
    private int level;

    public StatsPool Type { get { return type; } set { type = value; } }
    public int Point { get { return point; } set { point = value; } }
    public int Level { get { return level; } set { level = value; } }
}
