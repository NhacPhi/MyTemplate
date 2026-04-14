using TMPro;
using UnityEngine;
using System.Collections.Generic;
using VContainer;
using System.Linq;

public class ArmorCardInforUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtNameItem;

    [SerializeField] private List<ArmorStatsUI> armorStats;
    [SerializeField] private TextMeshProUGUI txtTitleSet;
    [SerializeField] private TextMeshProUGUI txtDescriptionSet;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;

    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateArmorItemCardInfor;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateArmorItemCardInfor;
    }

    public void UpdateArmorItemCardInfor(string id)
    {
        ArmorSaveData item = save.Player.Inventory.GetArmor(id);

        var itemConfig = gameDataBase.GetItemConfig(item.TemplateID);

        if(itemConfig != null)
        {
            txtNameItem.text = Utility.GetArmorPartName(itemConfig.Armor.Part) + " " + Utility.GetArmorRaretName(item.Rare)
                + "-" + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);

            ResetArmorStatsUI();

            armor.Init(item.UUID, item.Rare, itemConfig.Icon, gameDataBase.GetBGItemByRare(item.Rare), item.Level);
            armor.CanClick = false;
            if (item.Substats.Count > 0)
            {
                foreach (var obj in item.Substats)
                {
                    UpdateArmorStatsUI(obj);
                }
            }

            var mainstat = gameDataBase.GetItemConfig(item.TemplateID).Armor.MainStat;
            var mainstatUI = armorStats.FirstOrDefault(u => u.Type == mainstat.Type);
            mainstatUI.gameObject.SetActive(true);
            mainstatUI.gameObject.transform.SetAsFirstSibling();
            mainstatUI.UpdateStat((int)mainstat.Value, item.Level);

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
