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

    public void SetValue(float damage)
    {
        string textDmg = Mathf.CeilToInt(damage).ToString(CultureInfo.InvariantCulture);
        TMP.text = textDmg;
    }
}
