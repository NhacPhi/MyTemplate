using System;

enum SkillType
{
    Damage,
    Heal,
    Shield,
}

enum TargetArenaType
{
    Single,
    Aoe,
    Column,
    Row
}

public class SkillConfig
{
    private string id;
    private string name;
    private string description;
    private float value;

}
