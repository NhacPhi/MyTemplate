using System;
using UnityEngine;

[Serializable]
public class ExpConfig : ItemBaseConfig
{
    private int exp;

    public int Exp { get { return exp; } set { exp = value; } }
}
