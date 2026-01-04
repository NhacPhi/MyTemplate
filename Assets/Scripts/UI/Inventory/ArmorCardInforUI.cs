using TMPro;
using UnityEngine;
using System.Collections.Generic;
using VContainer;

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
        ArmorSaveData item = save.Player.GetArmor(id);

        var itemConfig = gameDataBase.GetItemConfig(id);

        if(itemConfig != null)
        {
            txtNameItem.text = Utility.GetArmorPartName(itemConfig.Armor.Part) + " " + Utility.GetArmorRaretName(item.Rare)
                + "-" + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);

            ResetArmorStatsUI();

            armor.Init(item.InstanceID, item.Rare, itemConfig.Icon, itemConfig.IconBG, item.Level);
            armor.CanClick = false;
            if (item.Stats.Count > 0)
            {
                foreach (var obj in item.Stats)
                {
                    UpdateArmorStatsUI(obj);
                }
            }

            txtTitleSet.text = "ATK Set(0/6)";
            txtDescriptionSet.text = "Increasece ATK by 20%";
        }


    }

    private void ResetArmorStatsUI()
    {
        foreach(var obj in armorStats)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateArmorStatsUI(ArmorStatSaveData stats)
    {
        foreach(var armor in armorStats)
        {
            if(armor.Type == stats.Type)
            {
                armor.gameObject.SetActive(true);
                armor.UpdateStat(stats.Point, stats.Level);
            }
        }
    }
}
