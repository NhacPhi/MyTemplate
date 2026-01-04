using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;
public class WeaponAscendCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [SerializeField] private TextMeshProUGUI txtCurentHP;
    [SerializeField] private TextMeshProUGUI txtCurentATK;

    [SerializeField] private TextMeshProUGUI txtNameAndLevel;
    [SerializeField] private TextMeshProUGUI txtUse;
    [SerializeField] private TextMeshProUGUI txtCoin;

    [Inject] GameDataBase gameDataBase;
    [Inject] SaveSystem save;

    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdateWeaponAscendCard;
    }


    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdateWeaponAscendCard;
    }

    public void UpdateWeaponAscendCard(string weaponID)
    {
        if (weaponID != "")
        {
            ItemConfig config = gameDataBase.GetItemConfig(weaponID);
            WeaponSaveData data = save.Player.GetWeapon(weaponID);

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = data.CurrentLevel;
            txtLevel.text = level.ToString() + "/10";

            int currentHP = config.Weapon.Stats.GetValueOrDefault(StatType.HP) 
                + Utility.GetStatGrowthLevel(level, config.Weapon.Upgrades.GetValueOrDefault(StatType.HP));
            int currentATK = config.Weapon.Stats.GetValueOrDefault(StatType.ATK) 
                + Utility.GetStatGrowthLevel(level + 1, config.Weapon.Upgrades.GetValueOrDefault(StatType.ATK));

            txtCurentHP.text = currentHP.ToString();
            txtCurentATK.text = currentATK.ToString();

            txtNameAndLevel.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + "(Lv." + data.CurrentUpgrade.ToString() + ")";
            txtUse.text = LocalizationManager.Instance.GetLocalizedValue(config.UseDescription);
            txtCoin.text = Utility.GetCoinNeedToAsscendWeapon(data.CurrentUpgrade).ToString();
        }
    }
}
