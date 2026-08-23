using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmorStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtStats;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image icon;
    [SerializeField] private StatType type;

    public StatType Type => type;

    private void Awake()
    {
        AutoFindComponents();
    }

    private void AutoFindComponents()
    {
        if (txtName == null)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t != txtStats && t != txtLevel)
                {
                    txtName = t;
                    break;
                }
            }
        }

        if (icon == null)
        {
            icon = GetComponentInChildren<Image>(true);
        }
    }

    public void UpdateStat(StatType statType, int value, int level, ModifyType modType = ModifyType.Flat, GameDataBase gameData = null)
    {
        type = statType;
        AutoFindComponents();

        if (txtLevel != null) 
        {
            txtLevel.text = level > 1 ? $"+{level}" : level.ToString();
        }
        
        string valStr = Utility.GetConvertStatValueToString(value, modType, statType);
        if (txtStats != null) 
        {
            txtStats.text = valStr;
        }

        if (txtName != null)
        {
            txtName.text = Utility.GetContextByStatType(statType);
        }

        if (icon != null && gameData != null)
        {
            var sp = gameData.GetStatIcon(statType);
            if (sp != null)
            {
                icon.sprite = sp;
                icon.gameObject.SetActive(true);
            }
        }
    }

    public void UpdateStat(int value, int level, ModifyType modType = ModifyType.Flat)
    {
        UpdateStat(type, value, level, modType, null);
    }
}
