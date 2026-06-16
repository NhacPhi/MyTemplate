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

    [SerializeField] private GameObject upgradeOb;
    [SerializeField] private GameObject readedOb;
    [SerializeField] private GameObject limitBreakOb;
    [SerializeField] private ItemUI limitBreakItemUI;
    [SerializeField] private TextMeshProUGUI txtLimitBreakCoin;
    [SerializeField] private Button btnLimitBreak;

    [Inject] GameDataBase gameDataBase;
    [Inject] InventoryManager inventory;
    [Inject] CurrencyManager currencyMM;
    [Inject] ForgeManager forgeManager;
    [Inject] PlayerCharacterManager playerCharacterManager;

    private string currentWeaponUUID;

    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdatedWeaponUpgradeCard;

        if (btnUpdateLevel != null)
            btnUpdateLevel.onClick.AddListener(OnBtnUpdateLevelClicked);
        if (UpdateToLevel != null)
            UpdateToLevel.onClick.AddListener(OnBtnUpdateLevelToClicked);
        if (btnLimitBreak != null)
            btnLimitBreak.onClick.AddListener(OnBtnLimitBreakClicked);
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

    private void OnBtnLimitBreakClicked()
    {
        if (!string.IsNullOrEmpty(currentWeaponUUID))
        {
            forgeManager.LimitBreakWeapon(currentWeaponUUID);
        }
    }

    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdatedWeaponUpgradeCard;
    }
    public void UpdatedWeaponUpgradeCard(string weaponUUID)
    {
        if (weaponUUID != "")
        {
            currentWeaponUUID = weaponUUID;
            var weaponSave = inventory.GetWeapon(weaponUUID);
            var config = gameDataBase.GetItemConfig(weaponSave.TemplateID);

            string ascensionID = Utility.GetAscentionConfigIDByWeaponRare(config.Rarity);
            int maxLevel = gameDataBase.GetWeaponMaxLevel(ascensionID, weaponSave.CurrentLevel);

            bool canLimitBreak = false;

            if (weaponSave.CurrentLevel < maxLevel)
            {
                upgradeOb.gameObject.SetActive(true);
                readedOb.gameObject.SetActive(false);
                if (limitBreakOb != null) limitBreakOb.SetActive(false);
            }
            else
            {
                var ascensionConfig = gameDataBase.GetAscensionConfig(ascensionID);
                TierConfig currentTier = null;
                if (ascensionConfig != null && ascensionConfig.TierConfigs != null)
                {
                    foreach (var tier in ascensionConfig.TierConfigs.Values)
                    {
                        if (tier.LevelRequire == weaponSave.CurrentLevel)
                        {
                            currentTier = tier;
                            break;
                        }
                    }
                }

                if (currentTier != null)
                {
                    canLimitBreak = true;
                    upgradeOb.gameObject.SetActive(false);
                    readedOb.gameObject.SetActive(false);
                    if (limitBreakOb != null) limitBreakOb.SetActive(true);

                    // Show cost
                    if (currentTier.costs != null)
                    {
                        foreach(var cost in currentTier.costs)
                        {
                            if (cost.ID == "Coin")
                            {
                                if (txtLimitBreakCoin != null) txtLimitBreakCoin.text = Utility.FormatCurrency(cost.Quantity);
                            }
                            else
                            {
                                if (limitBreakItemUI != null)
                                {
                                    var itemConfig = gameDataBase.GetItemConfig(cost.ID);
                                    if (itemConfig != null)
                                    {
                                        var ownAmount = inventory.GetItemQuantity(cost.ID);
                                        limitBreakItemUI.InitRequirement(cost.ID, itemConfig.Rarity, itemConfig.Icon, gameDataBase.GetBGItemByRare(itemConfig.Rarity), ownAmount, cost.Quantity);
                                        limitBreakItemUI.ActiveFragIcon(itemConfig.Type == ItemType.Shard);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    upgradeOb.gameObject.SetActive(false);
                    readedOb.gameObject.SetActive(true);
                    if (limitBreakOb != null) limitBreakOb.SetActive(false);
                }
            }

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = weaponSave.CurrentLevel;
            txtLevel.text = level.ToString();

            bool showNext = (level < maxLevel) || canLimitBreak;

            txtNextLevel.text = showNext ? (level + 1).ToString() :
                LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");

            int currentHP = config.Weapon.GetStatByLevel(StatType.HP, level);
            int nextHP = config.Weapon.GetStatByLevel(StatType.HP, level + 1);

            int currentATK = config.Weapon.GetStatByLevel(StatType.ATK, level);
            int nextATK = config.Weapon.GetStatByLevel(StatType.ATK, level + 1);

            txtCurentHP.text = currentHP.ToString();
            txtCurrentATK.text = currentATK.ToString();

            txtNextHP.text = showNext ? nextHP.ToString() : currentHP.ToString();
            txtNextATK.text = showNext ?  nextATK.ToString() : currentATK.ToString();

            uiUpgrade.UpdateUI(weaponSave.CurrentUpgrade);

            var currentEssence = currencyMM.GetQuantityCurrecy(CurrencyType.RelicEssence);

            var singleReq = forgeManager.GetUpgradeRequirementsFormatted(weaponUUID);
            txtRelicEsscence.text = singleReq.essenceStr + " / " + Utility.FormatCurrency(currentEssence);

            var maxReq = forgeManager.GetMaxUpgradeRequirementsFormatted(weaponUUID);
            
            // Sửa logic: Nếu không đủ tiền/tài nguyên để lên dù chỉ 1 cấp, fallback mục tiêu Max level trên nút là cấp tiếp theo (chứ không phải cấp hiện tại)
            var levelUpTo = level + (maxReq.levelsUp > 0 ? maxReq.levelsUp : 1);

            if (level >= maxLevel) 
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
