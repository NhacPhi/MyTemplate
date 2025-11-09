using System;


[Serializable]
public class WeaponConfig : ItemBaseConfig
{
    private CharacterType type;
    private string skillDes;
    private int hp;
    private int atk;

    public CharacterType Type { get { return type; } set { type = value; } }
    public string SkillDes { get { return skillDes; } set { skillDes = value; } }
    public int HP { get { return hp; } set { hp = value; } }
    public int Atk { get { return atk; } set { atk = value; } }
}
