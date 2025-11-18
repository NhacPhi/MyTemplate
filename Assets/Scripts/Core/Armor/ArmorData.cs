using System.Collections;
using System.Collections.Generic;

public class ArmorData
{
    private string instanceID;
    private string templateID;
    private int level;
    private Rare rare;
    private List<ArmorStats> stats;
    private string equip;
    public string InstanceID { get { return instanceID; } set { instanceID = value; } }
    public string TemplateID { get { return templateID; } set { templateID = value; } }
    public Rare Rare { get { return rare; } set { rare = value; } }
    public int Level { get { return level; } set { level = value; } }
    public List<ArmorStats> Stats { get { return stats; } set { stats = value; } }
    public string Equip { get { return equip; } set { equip = value; } }
}
