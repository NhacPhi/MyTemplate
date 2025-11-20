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
    [Inject] SaveSystem save;
    [Inject] CurrencyManager currencyMM;
    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdatedWeaponUpgradeCard;
    }


    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdatedWeaponUpgradeCard;
    }
    public void UpdatedWeaponUpgradeCard(string weaponID)
    {
        if(weaponID != "")
        {
            WeaponConfig config = gameDataBase.GetWeaponConfig(weaponID);
            WeaponData data = save.Player.GetWeapon(weaponID);
            WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, weaponID);

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = data.CurrentLevel;
            txtLevel.text = level.ToString() + "/10";
            txtNextLevel.text = level < 10 ? ((level + 1).ToString() + "/10") :
                LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");

            int currentHP = config.HP + Utility.GetStatGrowthLevel(level, config.GrowthHP);
            int nextHP = config.HP + Utility.GetStatGrowthLevel(level + 1, config.GrowthHP);

            int currentATK = config.ATK + Utility.GetStatGrowthLevel(level, config.GrowthATK);
            int nextATK = config.ATK + Utility.GetStatGrowthLevel(level + 1, config.GrowthATK);

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
