using TMPro;
using UnityEngine;

public class ArmorStatsUI : MonoBehaviour
{
    [SerializeReference] private TextMeshProUGUI txtStats;
    [SerializeReference] private TextMeshProUGUI txtLevel;
    [SerializeReference] private StatType type;

    public StatType Type => type;

    public void UpdateStat(int value, int level)
    {
        txtLevel.text = level.ToString();
        txtStats.text = value.ToString();
    }
}
