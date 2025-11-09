using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponData
{
    private string id;
    private int currentLevel;
    private int currentUpgrade;

    public string ID { get { return id; } set { id = value; } }
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }
    public int CurrentUpgrade { get { return currentUpgrade; } set { currentUpgrade = value; } }
}
