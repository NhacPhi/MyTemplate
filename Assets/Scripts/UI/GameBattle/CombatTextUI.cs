using System.Globalization;
using TMPro;
using UnityEngine;

public class CombatTextUI : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI TMP { get; private set; }
    private void Reset()
    {
        LoadText();
    }

    private void Awake()
    {
        LoadText();
    }

    private void LoadText()
    {
        if (TMP) return;

        TMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    [SerializeField] private GameObject iconCritical;

    public void SetValue(float damage)
    {
        string textDmg = Mathf.CeilToInt(damage).ToString(CultureInfo.InvariantCulture);
        TMP.text = textDmg;
    }

    public void SetCritical(bool isCritical)
    {
        if (iconCritical != null)
        {
            iconCritical.SetActive(isCritical);
        }
    }
}
