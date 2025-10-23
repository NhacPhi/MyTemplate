using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FoodConfig : ItemBaseConfig
{
    private string useful;

    public string Useful { get { return useful; } set { useful = value; } }
}
