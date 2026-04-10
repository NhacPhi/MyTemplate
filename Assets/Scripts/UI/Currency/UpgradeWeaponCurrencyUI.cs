using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeWeaponCurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtEssence;

    private void OnEnable()
    {
        UIEvent.OnCurrencyChanged += UpdateCurrency;
    }

    private void OnDisable()
    {
        UIEvent.OnCurrencyChanged -= UpdateCurrency;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void UpdateCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Coin:
                txtCoin.text = Utility.FormatCurrency(amount);
                break;
            case CurrencyType.RelicEssence:
                txtEssence.text = Utility.FormatCurrency(amount);
                break;
            default: break;
        }
    }
}
