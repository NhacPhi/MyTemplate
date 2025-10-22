using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponUI : InventoryItemUI
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private GameObject parent;
    private UpgradeUI[] upgrades;

    private void Awake()
    {
        upgrades = parent.GetComponentsInChildren<UpgradeUI>();
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = level.ToString();
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (i < upgradeNumber)
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
