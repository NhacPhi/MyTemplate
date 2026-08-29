using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesUI : MonoBehaviour
{
    [SerializeField] private UpgradeUI[] upgrades;
    public void UpdateUI(int upgrade)
    {
        if (upgrades == null) return;
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (upgrades[i] == null) continue;
            if (i < upgrade)
            {
                upgrades[i].ActiveLayer(1);
            }
            else
            {
                upgrades[i].ActiveLayer(0);
            }
        }
    }
}
