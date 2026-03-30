using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class ArmorTooltipUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtScore;

    [SerializeReference] private List<ArmorStatsUI> armorStats;
    [SerializeField] private TextMeshProUGUI txtTitleSet;
    [SerializeField] private TextMeshProUGUI txtDescriptionSet;

    [Inject] GameDataBase gameDataBase;
    [Inject] InventoryManager inventoryManager;
    private void Awake()
    {
        UIEvent.OnUpdateArmorTooltipUI += UpdateArmorUI;
    }


    private void OnEnable()
    {
        UIEvent.OnHideAllToolTipUI += Hide;
    }

    private void OnDisable()
    {
        UIEvent.OnHideAllToolTipUI -= Hide;
    }

    private void OnDestroy()
    {
        UIEvent.OnUpdateArmorTooltipUI -= UpdateArmorUI;
    }


    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateArmorUI(string armorID)
    {
        ArmorSaveData armorSaveData = inventoryManager.GetArmor(armorID);
        ItemConfig armorConfig = gameDataBase.GetItemConfig(armorSaveData.TemplateID);
        
        armor.GetComponent<ArmorItemUI>().Init(armorSaveData.UUID, armorSaveData.Rare, armorConfig.Icon, 
            gameDataBase.GetBGItemByRare(armorSaveData.Rare), armorSaveData.Level);
        armor.CanClick = false;

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(armorConfig.Name);
        txtScore.text = "100";

        ResetArmorStatsUI();

        if (armorSaveData.Substats.Count > 0)
        {
            foreach (var obj in armorSaveData.Substats)
            {
                UpdateArmorSubstatsUI(obj);
            }
        }

        var mainstat = gameDataBase.GetItemConfig(armorSaveData.TemplateID).Armor.MainStat;

        var mainstatUI = armorStats.FirstOrDefault(u => u.Type == mainstat.Type);
        mainstatUI.gameObject.SetActive(true);
        mainstatUI.gameObject.transform.SetAsFirstSibling();
        mainstatUI.UpdateStat((int)mainstat.Value, armorSaveData.Level);

        txtTitleSet.text = "ATK Set(0/6)";
        txtDescriptionSet.text = "Increasece ATK by 20%";

        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
    }
    private void ResetArmorStatsUI()
    {
        foreach (var obj in armorStats)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateArmorSubstatsUI(RolledSubStat stats)
    {
        foreach (var armor in armorStats)
        {
            if (armor.Type == stats.Type)
            {
                armor.gameObject.SetActive(true);
                armor.UpdateStat(stats.Value, stats.Level);
            }
        }
    }

}
