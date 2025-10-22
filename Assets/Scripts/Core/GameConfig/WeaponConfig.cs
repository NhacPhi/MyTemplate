using System;


[Serializable]
public class WeaponConfig
{
    private string id;
    private string name;
    private string description;
    private WeaponType type;
    private Rare rare;
    private string skillDes;
    private int hP;
    private int atk;

    public string ID { get { return id; }  set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Description { get { return description; } set { description = value; } }
    public WeaponType Type { get { return type; } set { type = value; } }
    public Rare Rare { get { return rare; } set { rare = value; } }
    public string SkillDes { get { return skillDes; } set { skillDes = value; } }
    public int HP { get { return hP; } set { hP = value; } }
    public int Atk { get { return atk; } set { atk = value; } }
}
