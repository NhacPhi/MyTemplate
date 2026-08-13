using TMPro;
using UnityEngine;

public class ArmorStatsUI : MonoBehaviour
{
    [SerializeReference] private TextMeshProUGUI txtStats;
    [SerializeReference] private TextMeshProUGUI txtLevel;
    [SerializeReference] private StatType type;

    public StatType Type => type;

    public void UpdateStat(int value, int level, ModifyType modType = ModifyType.Flat)
    {
        txtLevel.text = level.ToString();
        txtStats.text = modType == ModifyType.Percent ? $"{value}%" : value.ToString();
    }
}
