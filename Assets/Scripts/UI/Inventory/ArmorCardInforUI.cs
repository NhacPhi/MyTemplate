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

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;

    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }


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
        ArmorData item = save.Player.GetArmor(id);
        BaseArmorConfig config = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, item.TemplateID);
        ArmorSO armorSO = gameDataBase.GetItemSOByID<ArmorSO>(ItemType.Armor, item.TemplateID);

        txtNameItem.text = Utility.GetArmorPartName(config.Part) + " " + Utility.GetArmorRaretName(item.Rare) + "-" + LocalizationManager.Instance.GetLocalizedValue(config.Name);

        ResetArmorStatsUI();

        armor.Init(item.InstanceID, item.Rare, armorSO.Icon, gameDataBase.GetRareBG(item.Rare), item.Level);
        armor.CanClick = false;
        if(item.Stats.Count > 0)
        {
            foreach(var obj in item.Stats)
            {
                UpdateArmorStatsUI(obj);
            }
        }

        txtTitleSet.text = "ATK Set(0/6)";
        txtDescriptionSet.text = "Increasece ATK by 20%";
    }

    private void ResetArmorStatsUI()
    {
        foreach(var obj in armorStats)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateArmorStatsUI(ArmorStats stats)
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
