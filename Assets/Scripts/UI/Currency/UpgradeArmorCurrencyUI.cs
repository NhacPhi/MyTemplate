using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeArmorCurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtNumberArmorPrimorite;

    private void OnEnable()
    {
        UIEvent.OnCurrencyChanged += UpdateCurrency;
    }

    private void OnDisable()
    {
        UIEvent.OnCurrencyChanged -= UpdateCurrency;
    }

    public void UpdateCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Coin:
                txtCoin.text = Utility.FormatCurrency(amount);
                break;
            case CurrencyType.RelicEssence:
                txtNumberArmorPrimorite.text = Utility.FormatCurrency(amount);
                break;
            default: break;
        }
    }
}
