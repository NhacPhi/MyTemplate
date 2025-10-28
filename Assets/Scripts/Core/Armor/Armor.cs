using System.Collections;
using System.Collections.Generic;

public class Armor
{
    private string instanceID;
    private string templateID;
    private int level;
    private Rare rare;
    private Dictionary<StatPool, int> baseStats;
    public string InstanceID { get { return instanceID; } set { instanceID = value; } }
    public string TemplateID { get { return templateID; } set { templateID = value; } }
    public Rare Rare { get { return rare; } set { rare = value; } }
    public int Level { get { return level; } set { level = value; } }
    public Dictionary<StatPool, int> BaseStats { get { return baseStats; } set { baseStats = value; } }
}
