using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
public class WeaponUpgradeCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtNextLevel;
    [SerializeField] private UpgradesUI uiUpgrade;

    [SerializeField] private TextMeshProUGUI txtCurentHP;
    [SerializeField] private TextMeshProUGUI txtNextHP;

    [SerializeField] private TextMeshProUGUI txtCurrentATK;
    [SerializeField] private TextMeshProUGUI txtNextATK;

    [SerializeField] private TextMeshProUGUI txtRelicEsscence;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtCoinMaxLevel;

    [SerializeField] private TextMeshProUGUI txtBtnUpgradeTo;

    [Inject] GameDataBase gameDataBase;
    [Inject] InventoryManager inventory;
    [Inject] CurrencyManager currencyMM;
    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdatedWeaponUpgradeCard;
    }


    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdatedWeaponUpgradeCard;
    }
    public void UpdatedWeaponUpgradeCard(string weaponUUID)
    {
        if(weaponUUID != "")
        {
            var weaponSave = inventory.GetWeapon(weaponUUID);
            var config = gameDataBase.GetItemConfig(weaponSave.TemplateID);

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = weaponSave.CurrentLevel;
            txtLevel.text = level.ToString();
            txtNextLevel.text = level < 100 ? (level + 1).ToString() :
                LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");

            int currentHP = config.Weapon.Stats.GetValueOrDefault(StatType.HP) +
                Utility.GetStatGrowthLevel(level, config.Weapon.Upgrades.GetValueOrDefault(StatType.HP));
            int nextHP = config.Weapon.Stats.GetValueOrDefault(StatType.HP)
                + Utility.GetStatGrowthLevel(level + 1, config.Weapon.Upgrades.GetValueOrDefault(StatType.HP));

            int currentATK = config.Weapon.Stats.GetValueOrDefault(StatType.ATK) +
                Utility.GetStatGrowthLevel(level, config.Weapon.Upgrades.GetValueOrDefault(StatType.ATK));
            int nextATK = config.Weapon.Stats.GetValueOrDefault(StatType.ATK)
                + Utility.GetStatGrowthLevel(level + 1, config.Weapon.Upgrades.GetValueOrDefault(StatType.ATK));

            txtCurentHP.text = currentHP.ToString();
            txtCurrentATK.text = currentATK.ToString();

            txtNextHP.text = nextHP.ToString();
            txtNextATK.text = nextATK.ToString();

            uiUpgrade.UpdateUI(weaponSave.CurrentUpgrade);

            var currentEssence = currencyMM.GetQuantityCurrecy(CurrencyType.RelicEssence);

            var levelUp = Utility.GetMaxLevelWithEssence(currentEssence);

            txtRelicEsscence.text = Utility.FormatCurrency(Utility.GetEssenceNeedToUpgradeWeapon(level + 1) - 
                Utility.GetEssenceNeedToUpgradeWeapon(level)) + " / " + currentEssence.ToString();

            var levelUpTo = level + levelUp;

            txtBtnUpgradeTo.text = LocalizationManager.Instance.GetLocalizedValue("UI_UPGRADE_MAX_LEVEL") + " " + levelUpTo.ToString();

            txtCoin.text = Utility.FormatCurrency(Utility.GetCoinNeedToUpgradeWeapon(level + 1) - Utility.GetCoinNeedToUpgradeWeapon(level));

            txtCoinMaxLevel.text = levelUpTo > level ? Utility.FormatCurrency(Utility.GetCoinNeedToUpgradeWeapon(levelUpTo) - Utility.GetCoinNeedToUpgradeWeapon(level)) : txtCoin.text;
        }
    }
}
