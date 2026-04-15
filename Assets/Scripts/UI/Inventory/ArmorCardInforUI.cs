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
        currentArmorPart = id;

        ArmorSaveData item = inventory.GetArmor(id);

        var itemConfig = gameDataBase.GetItemConfig(item.TemplateID);

        if(itemConfig != null)
        {
            txtNameItem.text = Utility.GetArmorPartName(itemConfig.Armor.Part) + " " + Utility.GetArmorRaretName(item.Rare)
                + "-" + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);

            ResetArmorStatsUI();

            // Main Stat 
            var mainStat = itemConfig.Armor.MainStat;
            iconMainStat.sprite = gameDataBase.GetStatIcon(mainStat.Type);
            txtStatType.text = Utility.GetContextByStatType(mainStat.Type);
            txtStatValue.text = Utility.GetArmorMainStatByLevel(mainStat.Value, item.Level).ToString();

            armor.Init(item.UUID, item.Rare, itemConfig.Icon, gameDataBase.GetBGItemByRare(item.Rare), item.Level);
            armor.CanClick = false;
            if (item.Substats.Count > 0)
            {
                foreach (var obj in item.Substats)
                {
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
                armor.UpdateStat(stats.Value, stats.Level);
            }
        }
    }
}
