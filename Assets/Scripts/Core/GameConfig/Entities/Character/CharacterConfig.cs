using System;
using UnityEngine;

public class CharacterConfig
{
    private string id;
    private string name;
    private CharacterRare rare;
    private CharacterType type;
    private string firstSkill;
    private string secondSkill;
    private string thirdSkill;



    public string ID { get { return id; } set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public CharacterRare Rare { get { return rare; } set { rare = value; } }
    public CharacterType Type { get { return type; } set { type = value; } }
    public string FirstSkill { get { return firstSkill; } set { firstSkill = value; } }
    public string SecondSkill { get { return secondSkill; } set { secondSkill = value; } }
    public string ThirdSkill { get { return thirdSkill; } set { thirdSkill = value; } }
}
