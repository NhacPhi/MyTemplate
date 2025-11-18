using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using static UnityEditor.Progress;

public class ArmorTooltipUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtScore;

    [SerializeReference] private List<ArmorStatsUI> armorStats;
    [SerializeField] private TextMeshProUGUI txtTitleSet;
    [SerializeField] private TextMeshProUGUI txtDescriptionSet;

    [Inject] IObjectResolver _resolver;
    [Inject] GameDataBase gameDataBase;
    [Inject] SaveSystem save;
    private void Awake()
    {
        UIEvent.OnUpdateArmorTooltipUI += UpdateArmorUI;
    }

    private void Start()
    {
        _resolver.Inject(this);
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
        ArmorData armorData = save.Player.GetArmor(armorID);
        BaseArmorConfig config = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, armorData.TemplateID);
        ArmorSO armorSO = gameDataBase.GetItemSOByID<ArmorSO>(ItemType.Armor, armorData.TemplateID);
        
        armor.GetComponent<ArmorItemUI>().Init(armorData.InstanceID, armorData.Rare, armorSO.Icon, gameDataBase.GetRareBG(armorData.Rare), armorData.Level);
        armor.CanClick = false;

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtScore.text = "100";

        ResetArmorStatsUI();

        if (armorData.Stats.Count > 0)
        {
            foreach (var obj in armorData.Stats)
            {
                UpdateArmorStatsUI(obj);
            }
        }

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

    private void UpdateArmorStatsUI(ArmorStats stats)
    {
        foreach (var armor in armorStats)
        {
            if (armor.Type == stats.Type)
            {
                armor.gameObject.SetActive(true);
                armor.UpdateStat(stats.Point, stats.Level);
            }
        }
    }
}
