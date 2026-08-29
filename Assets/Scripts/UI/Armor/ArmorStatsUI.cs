using TMPro;
using UnityEngine;

public class ArmorStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtStats;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private StatType type;

    public StatType Type => type;

    public void UpdateStat(int value, int level, ModifyType modType = ModifyType.Flat)
    {
        if (txtLevel != null)
        {
            txtLevel.text = level.ToString();
        }
        if (txtStats != null)
        {
            txtStats.text = modType == ModifyType.Percent ? $"{value}%" : value.ToString();
        }
    }
}
