using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtEnergy;
    [SerializeField] private TextMeshProUGUI txtJade;
    [SerializeField] private TextMeshProUGUI txtCoin;

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
        switch(type)
        {
            case CurrencyType.Energy: txtEnergy.text = Utility.FormatCurrency(amount);
                break;
            case CurrencyType.Jade: txtJade.text = Utility.FormatCurrency(amount);
                break;
            case CurrencyType.Coin: txtCoin.text = Utility.FormatCurrency(amount);
                break;
            default: break;
        }
    }
}
