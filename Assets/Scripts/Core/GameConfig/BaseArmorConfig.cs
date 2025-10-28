using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseArmorConfig : ItemBaseConfig
{
    private ArmorPart partArmor;
    private string setArmor;

    public ArmorPart Part { get { return partArmor; } set { partArmor = value; } }
    public string SetArmor { get { return setArmor; }  set { setArmor = value; } } 

}
