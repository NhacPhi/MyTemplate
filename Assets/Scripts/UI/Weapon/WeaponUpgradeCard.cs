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

    [SerializeField] private TextMeshProUGUI txtCurentHP;
    [SerializeField] private TextMeshProUGUI txtNextHP;

    [SerializeField] private TextMeshProUGUI txtCurrentATK;
    [SerializeField] private TextMeshProUGUI txtNextATK;

    [SerializeField] private TextMeshProUGUI txtRelicEsscence;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtCoinMaxLevel;

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
            txtLevel.text = level.ToString() + "/10";
            txtNextLevel.text = level < 10 ? ((level + 1).ToString() + "/10") :
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

            txtRelicEsscence.text = currencyMM.GetQuantityCurrecy(CurrencyType.RelicEssence).ToString() + "/" + Utility.GetEssenceNeedToUpgradeWeapon(level).ToString();
            txtCoin.text = Utility.GetCoinNeedToUpgradeWeapon(level).ToString();
            txtCoinMaxLevel.text = (Utility.GetCoinNeedToUpgradeWeapon(10) - Utility.GetCoinNeedToUpgradeWeapon(level)).ToString();
        }
    }
}
