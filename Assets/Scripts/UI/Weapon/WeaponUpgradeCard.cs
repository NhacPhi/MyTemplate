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

    [SerializeField] private Button btnUpdateLevel;
    [SerializeField] private Button UpdateToLevel;

    [Inject] GameDataBase gameDataBase;
    [Inject] InventoryManager inventory;
    [Inject] CurrencyManager currencyMM;
    [Inject] ForgeManager forgeManager;

    private string currentWeaponUUID;

    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdatedWeaponUpgradeCard;

        if (btnUpdateLevel != null)
            btnUpdateLevel.onClick.AddListener(OnBtnUpdateLevelClicked);
        if (UpdateToLevel != null)
            UpdateToLevel.onClick.AddListener(OnBtnUpdateLevelToClicked);
    }

    private void OnBtnUpdateLevelClicked()
    {
        if (!string.IsNullOrEmpty(currentWeaponUUID))
        {
            forgeManager.UpgradeWeapon(currentWeaponUUID);
        }
    }

    private void OnBtnUpdateLevelToClicked()
    {
        if (!string.IsNullOrEmpty(currentWeaponUUID))
        {
            forgeManager.UpgradeWeaponMax(currentWeaponUUID);
        }
    }


    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdatedWeaponUpgradeCard;
    }
    public void UpdatedWeaponUpgradeCard(string weaponUUID)
    {
        if(weaponUUID != "")
        {
            currentWeaponUUID = weaponUUID;
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

            var singleReq = forgeManager.GetUpgradeRequirementsFormatted(weaponUUID);
            txtRelicEsscence.text = singleReq.essenceStr + " / " + Utility.FormatCurrency(currentEssence);

            var maxReq = forgeManager.GetMaxUpgradeRequirementsFormatted(weaponUUID);
            
            // Sửa logic: Nếu không đủ tiền/tài nguyên để lên dù chỉ 1 cấp, fallback mục tiêu Max level trên nút là cấp tiếp theo (chứ không phải cấp hiện tại)
            var levelUpTo = level + (maxReq.levelsUp > 0 ? maxReq.levelsUp : 1);

            if (level >= 100) 
            {
                txtBtnUpgradeTo.text = LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");
            }
            else
            {
                txtBtnUpgradeTo.text = LocalizationManager.Instance.GetLocalizedValue("UI_UPGRADE_MAX_LEVEL") + " " + levelUpTo.ToString();
            }

            txtCoin.text = singleReq.coinStr;
            txtCoinMaxLevel.text = maxReq.levelsUp > 0 ? maxReq.coinStr : txtCoin.text;
        }
    }
}
