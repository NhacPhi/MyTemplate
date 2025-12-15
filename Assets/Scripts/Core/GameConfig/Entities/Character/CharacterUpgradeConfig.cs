using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUpgradeConfig
{
    private string id;
    private float growthHP;
    private float grouthATK;
    private float growthDEF;

    public string ID { get { return id; } set { id = value; } }
    public float GrowthHP { get { return growthHP; } set { growthHP = value; } }
    public float GrowthDEF { get { return growthDEF; } set { growthDEF = value; } }
    public float GrowthATK { get { return grouthATK; } set { grouthATK = value; } }
}
