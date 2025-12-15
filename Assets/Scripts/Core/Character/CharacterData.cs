using System.Collections.Generic;
using System;

public class CharacterData
{
    private string id;
    private int level;
    private int exp;
    private int boostStats; //Get shard to upgrade
    private List<Part> armors;
    private string weapon;

    public string ID { get { return id; } set { id = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int Exp { get { return exp; } set { exp = value; } }
    public int BoostStats { get { return boostStats; } set { boostStats = value; } }
    public string Weapon { get { return weapon; } set { weapon = value; } }
    public List<Part> Armors { get { return armors; } set { armors = value; } }
}
