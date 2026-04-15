using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// UI chính cho hệ thống nâng cấp Armor.
/// Hiển thị thông tin armor, preview level mục tiêu, chi phí, và substats.
/// Hỗ trợ các nút: +1, -1, +3, -3, Upgrade.
/// </summary>
public class ArmorUpgradeCard : MonoBehaviour
{
    [Header("Armor Info")]
    [SerializeField] private ArmorItemUI armorItemUI;

    [SerializeField] private TextMeshProUGUI txtCurrentLevel;
    [SerializeField] private TextMeshProUGUI txtNextLevel;
    [SerializeField] private Slider progressbar;
    [SerializeField] private TextMeshProUGUI txtProgress;



    [Header("Main Stat")]
    // Icon main stat (đoạn này cần tạo hệ thống tham chiếu icon them Type)
    [SerializeField] private Image iconMainStat;
    [SerializeField] private TextMeshProUGUI txtMainStatName;
    [SerializeField] private TextMeshProUGUI txtCurrentMainStat;
    [SerializeField] private TextMeshProUGUI txtNextMainStat;
    [SerializeField] private TextMeshProUGUI bonusSetTitle;
    [SerializeField] private TextMeshProUGUI bonusSetContent;

    [Header("SubStats")]
    [SerializeField] private List<ArmorStatsUI> subStatSlots;

    [Header("Cost")]
    [SerializeField] private TextMeshProUGUI txtCoinCost;
    [SerializeField] private TextMeshProUGUI txtPrimoriteCost;
    [SerializeField] private TextMeshProUGUI txtCurrentPrimorite;

    [Header("Buttons")]
    [SerializeField] private Button btnPlus;
    [SerializeField] private Button btnMinus;
    [SerializeField] private Button btnPlusThree;
    [SerializeField] private Button btnMinusThree;
    [SerializeField] private Button btnUpgrade;
    [SerializeField] TextMeshProUGUI txtLevelTarget;

    [Header("State Objects")]
    [SerializeField] private GameObject upgradeOb;
    [SerializeField] private GameObject reachedOb;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private CurrencyManager currencyManager;
    [Inject] private ForgeManager forgeManager;

    private string currentArmorUUID;
    private int targetLevel;
    private Tween _progressTween;

    private void Awake()
    {
        UIEvent.OnSelectArmorUpgrade += OnSelectArmor;
        UIEvent.OnArmorUpgraded += OnArmorUpgraded;

        if (btnPlus != null) btnPlus.onClick.AddListener(() => AdjustTargetLevel(1));
        if (btnMinus != null) btnMinus.onClick.AddListener(() => AdjustTargetLevel(-1));
        if (btnPlusThree != null) btnPlusThree.onClick.AddListener(() => AdjustTargetLevel(3));
        if (btnMinusThree != null) btnMinusThree.onClick.AddListener(() => AdjustTargetLevel(-3));
        if (btnUpgrade != null) btnUpgrade.onClick.AddListener(OnBtnUpgradeClicked);
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectArmorUpgrade -= OnSelectArmor;
        UIEvent.OnArmorUpgraded -= OnArmorUpgraded;
    }

    /// <summary>
    /// Khi chọn 1 armor để nâng cấp
    /// </summary>
    private void OnSelectArmor(string armorUUID)
    {
        currentArmorUUID = armorUUID;

        var armorSave = inventoryManager.GetArmor(armorUUID);
        if (armorSave == null) return;

        // Reset target level = current + 1
        targetLevel = Mathf.Min(armorSave.Level + 1, Definition.MAX_ARMOR_LEVEL);
        txtLevelTarget.text = targetLevel.ToString();
        ResetProgress(armorSave.Level);
        UpdateUI();
    }

    /// <summary>
    /// Callback sau khi upgrade thành công - refresh UI
    /// </summary>
    private void OnArmorUpgraded(string armorUUID)
    {
        if (armorUUID == currentArmorUUID)
        {
            var armorSave = inventoryManager.GetArmor(armorUUID);
            if (armorSave != null)
            {
                targetLevel = Mathf.Min(armorSave.Level + 1, Definition.MAX_ARMOR_LEVEL);
            }
            UpdateUI();
            // Reset progress về 0 sau khi upgrade xong, sẵn sàng cho lần nâng cấp tiếp
            if (armorSave != null) ResetProgress(armorSave.Level);
        }
    }

    /// <summary>
    /// Điều chỉnh target level theo delta (+1, -1, +3, -3)
    /// </summary>
    private void AdjustTargetLevel(int delta)
    {
        if (string.IsNullOrEmpty(currentArmorUUID)) return;

        var armorSave = inventoryManager.GetArmor(currentArmorUUID);
        if (armorSave == null) return;

        targetLevel += delta;

        // Clamp: không dưới (currentLevel + 1), không trên MAX_ARMOR_LEVEL
        targetLevel = Mathf.Clamp(targetLevel, armorSave.Level + 1, Definition.MAX_ARMOR_LEVEL);

        UpdateCostPreview(armorSave.Level);
        UpdateStatPreview(armorSave.Level);
        ResetProgress(armorSave.Level);
    }

    /// <summary>
    /// Bấm nút Upgrade
    /// </summary>
    private void OnBtnUpgradeClicked()
    {
        if (string.IsNullOrEmpty(currentArmorUUID)) return;

        var armorSave = inventoryManager.GetArmor(currentArmorUUID);

        if (armorSave == null) return;
        if (armorSave.Level >= Definition.MAX_ARMOR_LEVEL) return;

        // Animate progress bar lên đầy trước, sau đó thực hiện upgrade
        AnimateProgress(armorSave.Level, () =>
        {
            forgeManager.UpgradeArmor(currentArmorUUID, targetLevel, gameDataBase);
        });
    }

    /// <summary>
    /// Cập nhật toàn bộ UI
    /// </summary>
    private void UpdateUI()
    {
        if (string.IsNullOrEmpty(currentArmorUUID)) return;

        var armorSave = inventoryManager.GetArmor(currentArmorUUID);
        if (armorSave == null) return;

        var config = gameDataBase.GetItemConfig(armorSave.TemplateID);
        if (config == null) return;

        // Kiểm tra max level
        bool isMaxLevel = armorSave.Level >= Definition.MAX_ARMOR_LEVEL;
        if (upgradeOb != null) upgradeOb.SetActive(!isMaxLevel);
        if (reachedOb != null) reachedOb.SetActive(isMaxLevel);

        // Level
        txtCurrentLevel.text = armorSave.Level.ToString();
        txtNextLevel.text = isMaxLevel
            ? LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL")
            : targetLevel.ToString();
        txtLevelTarget.text= targetLevel.ToString();

        armorItemUI.Init(armorSave.UUID, armorSave.Rare, config.Icon,
                            gameDataBase.GetBGItemByRare(armorSave.Rare), armorSave.Level);

        var bonusConfig = gameDataBase.GetSetBonusConfig(config.Armor.ArmorSet);
        bonusSetTitle.text = bonusConfig.GetTitleSetBonus();
        bonusSetContent.text = bonusConfig.GetConentBonus();

        // Main Stat
        UpdateMainStat();

        // SubStats
        UpdateSubStatsUI(armorSave, config);

        // Cost preview
        UpdateCostPreview(armorSave.Level);
    }

    /// <summary>
    /// Hiển thị substats hiện tại và indicator cho substat sắp mở khóa
    /// </summary>
    private void UpdateSubStatsUI(ArmorSaveData armorSave, ItemConfig config)
    {
        // Reset tất cả slot
        if (subStatSlots != null)
        {
            foreach (var slot in subStatSlots)
            {
                slot.gameObject.SetActive(false);
            }
        }

        if (armorSave.Substats == null || subStatSlots == null) return;

        // Hiển thị substats đã có
        for (int i = 0; i < armorSave.Substats.Count && i < subStatSlots.Count; i++)
        {
            var sub = armorSave.Substats[i];
            var slot = subStatSlots.FirstOrDefault(s => s.Type == sub.Type);
            if (slot != null)
            {
                slot.gameObject.SetActive(true);
                slot.UpdateStat(sub.Value, sub.Level);
            }
        }
    }

    /// <summary>
    /// Cập nhật chi phí preview dựa trên target level
    /// </summary>
    private void UpdateCostPreview(int currentLevel)
    {
        if (currentLevel >= Definition.MAX_ARMOR_LEVEL)
        {
            if (txtCoinCost != null) txtCoinCost.text = "0";
            if (txtPrimoriteCost != null) txtPrimoriteCost.text = "0";
            if (txtCurrentPrimorite != null)
                txtCurrentPrimorite.text = Utility.FormatCurrency(
                    currencyManager.GetQuantityCurrecy(CurrencyType.ArmorPrimorite));
            return;
        }

        var armorSave = inventoryManager.GetArmor(currentArmorUUID);

        var cost = forgeManager.GetUpgradeArmorCost(currentLevel, targetLevel, armorSave.Rare);

        if (txtCoinCost != null) txtCoinCost.text = Utility.FormatCurrency(cost.coin);
        if (txtPrimoriteCost != null) txtPrimoriteCost.text = Utility.FormatCurrency(cost.primorite);
        if (txtCurrentPrimorite != null)
            txtCurrentPrimorite.text = Utility.FormatCurrency(
                currencyManager.GetQuantityCurrecy(CurrencyType.ArmorPrimorite));
        if (txtLevelTarget != null)
            txtLevelTarget.text = targetLevel.ToString();
    }

    private void UpdateStatPreview(int currentLevel)
    {
        if(txtCurrentLevel != null) txtCurrentLevel.text = currentLevel.ToString();

        if(txtLevelTarget != null) txtNextLevel.text = targetLevel.ToString();

        UpdateMainStat();
    }

    private void UpdateMainStat()
    {
        var armorSave = inventoryManager.GetArmor(currentArmorUUID);
        if (armorSave == null) return;

        var config = gameDataBase.GetItemConfig(armorSave.TemplateID);
        if (config == null) return;

        // Main Stat
        if (config.Armor != null && config.Armor.MainStat != null)
        {
            var mainStat = config.Armor.MainStat;
            txtMainStatName.text = Utility.GetContextByStatType(mainStat.Type);

            iconMainStat.sprite = gameDataBase.GetStatIcon(mainStat.Type);

            int currentMainStatValue = Utility.GetArmorMainStatByLevel(mainStat.Value, armorSave.Level);

            int nextMainStatValue = Utility.GetArmorMainStatByLevel(mainStat.Value, targetLevel);

            txtCurrentMainStat.text = currentMainStatValue.ToString();
            txtNextMainStat.text = nextMainStatValue.ToString();
        }
    }

    /// <summary>
    /// Reset progress bar về 0 và cập nhật text "Lv.current / Lv.target".
    /// Gọi khi chọn armor, thay đổi target level, hoặc sau khi upgrade xong.
    /// </summary>
    private void ResetProgress(int currentLevel)
    {
        _progressTween?.Kill();

        if (progressbar != null) progressbar.value = 0f;

        if (txtProgress != null)
        {
            if (currentLevel >= Definition.MAX_ARMOR_LEVEL)
                txtProgress.text = LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");
            else
                txtProgress.text = $"Lv.{currentLevel} / Lv.{targetLevel}";
        }
    }

    /// <summary>
    /// Animate progress bar từ 0 lên 1 (đầy) khi bấm nút Upgrade.
    /// Sau khi animation xong sẽ gọi onComplete callback.
    /// </summary>
    private void AnimateProgress(int currentLevel, System.Action onComplete = null)
    {
        _progressTween?.Kill();

        if (progressbar != null)
        {
            progressbar.value = 0f;
            _progressTween = progressbar.DOValue(1f, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }
        else
        {
            onComplete?.Invoke();
        }

        if (txtProgress != null)
            txtProgress.text = $"Lv.{currentLevel} → Lv.{targetLevel}";
    }
}
