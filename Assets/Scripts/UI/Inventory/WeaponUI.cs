using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponUI : InventoryItemUI
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgrades;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = level.ToString();
        upgrades.UpdateUI(upgradeNumber);
    }    
}
