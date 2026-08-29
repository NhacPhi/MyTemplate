using TMPro;
using UnityEngine;
using System.Collections.Generic;
using VContainer;
using System.Linq;
using UnityEngine.UI;

public class ArmorCardInforUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtNameItem;

    [Header("MainStat")]
    [SerializeField] private Image iconMainStat;
    [SerializeField] private TextMeshProUGUI txtStatType;
    [SerializeField] private TextMeshProUGUI txtStatValue;

    [SerializeField] private List<ArmorStatsUI> armorStats;
    [SerializeField] private TextMeshProUGUI txtTitleSet;
    [SerializeField] private TextMeshProUGUI txtDescriptionSet;

    [SerializeField] private Button btnUpgrade;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventory;
    [Inject] private UIManager uiManager;
    private string currentArmorPart = "";
    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateArmorItemCardInfor;

            btnUpgrade.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.UpgradeArmorScene);

                UIEvent.OnSelectArmorUpgrade?.Invoke(currentArmorPart);
            });
        }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateArmorItemCardInfor;

        btnUpgrade.onClick.RemoveAllListeners();
    }

    public void UpdateArmorItemCardInfor(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        currentArmorPart = id;

        ArmorSaveData item = inventory.GetArmor(id);
        if (item == null) return;

        var itemConfig = gameDataBase.GetItemConfig(item.TemplateID);
        if (itemConfig == null) return;
        {
            txtNameItem.text = Utility.GetArmorPartName(itemConfig.Armor.Part) + " " + Utility.GetArmorRaretName(item.Rare)
                + "-" + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);

            ResetArmorStatsUI();

            // Main Stat 
            var mainStat = itemConfig.Armor.MainStat;
            StatType actualMainType = (item.MainStatType != StatType.None) 
                ? item.MainStatType 
                : (mainStat != null ? mainStat.Type : StatType.ATK);
            ModifyType modType = mainStat != null ? mainStat.ModifierType : ModifyType.Flat;
            float baseMainValue = Utility.GetAppropriateArmorMainBaseValue(actualMainType, modType, mainStat != null ? mainStat.Value : 0f);

            var statIcon = gameDataBase.GetStatIcon(actualMainType);
            if (iconMainStat != null)
            {
                iconMainStat.sprite = statIcon;
                iconMainStat.gameObject.SetActive(statIcon != null);
            }
            if (txtStatType != null)
            {
                txtStatType.text = Utility.GetContextByStatType(actualMainType);
            }
            float calculatedMainVal = Utility.GetArmorMainStatByLevel(baseMainValue, item.Level, item.Rare);
            if (txtStatValue != null)
            {
                txtStatValue.text = Utility.GetConvertStatValueToString(calculatedMainVal, modType, actualMainType);
            }

            armor.Init(item.UUID, item.Rare, itemConfig.Icon, gameDataBase.GetBGItemByRare(item.Rare), item.Level);
            armor.CanClick = false;

            SubstatPoolConfig poolConfig = null;
            if (!string.IsNullOrEmpty(itemConfig.Armor.SubstatPoolID))
            {
                poolConfig = gameDataBase.GetSubstatPoolConfig(itemConfig.Armor.SubstatPoolID);
            }

            if (item.Substats != null && item.Substats.Count > 0)
            {
                foreach (var obj in item.Substats)
                {
                    if (poolConfig != null && poolConfig.Pools != null)
                    {
                        var poolComp = poolConfig.Pools.Find(p => p.Type == obj.Type && p.ModifierType == obj.ModifierType)
                                    ?? poolConfig.Pools.Find(p => p.Type == obj.Type);
                        if (poolComp != null)
                        {
                            float avgPerRoll = (poolComp.Min + poolComp.Max) * 0.5f;
                            int calculatedVal = Mathf.RoundToInt(avgPerRoll * Mathf.Max(1, obj.Level));
                            obj.SetCalculatedValue(calculatedVal);
                        }
                    }
                    UpdateArmorStatsUI(obj);
                }
            }

            var setbonus = gameDataBase.GetSetBonusConfig(itemConfig.Armor.ArmorSet);
            txtTitleSet.text = setbonus.GetTitleSetBonus();
            txtDescriptionSet.text = setbonus.GetConentBonus();
        }


    }

    private void ResetArmorStatsUI()
    {
        foreach(var obj in armorStats)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateArmorStatsUI(RolledSubStat stats)
    {
        foreach(var armor in armorStats)
        {
            if(armor.Type == stats.Type)
            {
                armor.gameObject.SetActive(true);
                armor.UpdateStat(stats.Value, stats.Level, stats.ModifierType);
            }
        }
    }
}
