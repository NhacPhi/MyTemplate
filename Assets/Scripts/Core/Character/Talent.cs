using System;

public class Talent 
{
    private int hpPoint;
    private int atkPoint;
    private int defPoint;
    private int spdPoint;
    private int defShredPoint;
    private int critRatePoint;
    private int critDMGPoint;
    private int penetrationPoint;
    private int critDMGRes;

    public int HPPoint { get { return hpPoint; } set { hpPoint = value; } }
    public int ATKPoint { get { return atkPoint; } set { atkPoint = value; } }
    public int DEFPoin { get { return defPoint; } set { defPoint = value; } }
    public int SPDPoint { get { return spdPoint; } set { spdPoint = value; } }
    public int DEFShredPoint { get { return defShredPoint; } set { defShredPoint = value; } }
    public int CRITRatePoint { get { return critRatePoint; } set { critRatePoint = value; } }
    public int RITDMGPoint { get { return critDMGPoint; } set { critDMGPoint = value; } }
    public int PenetrationPoin { get { return penetrationPoint; } set { penetrationPoint = value; } }
    public int CIRTDMGRes { get { return critDMGRes; } set { critDMGRes = value;} }
}
