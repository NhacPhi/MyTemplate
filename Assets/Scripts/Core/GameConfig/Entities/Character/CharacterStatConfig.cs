using System;

public class CharacterStatConfig
{
    private string id;
    // Base Staff
    private float hp;
    private float atk;
    private float def;
    private float spd;
    private float defShred;
    private float critRate;
    private float critDMG;
    private float penetration;
    private float critDMGRes;

    public string ID { get { return id; } set { id = value; } }

    public float HP { get { return hp; } set { hp = value; } }
    public float ATK { get { return atk; } set { atk = value; } }
    public float DEF { get { return def; } set { def = value; } }
    public float SPD { get { return spd; } set { spd = value; } }
    public float DEFShred { get { return defShred; } set { defShred = value; } }
    public float CRITRate { get { return critRate; } set { critRate = value; } }
    public float CRITDMG { get { return critDMG; } set { critDMG = value; } }
    public float Penetration { get { return penetration; } set { penetration = value; } }
    public float CRITDMGRes { get { return critDMGRes; } set { critDMGRes = value; } }
}
