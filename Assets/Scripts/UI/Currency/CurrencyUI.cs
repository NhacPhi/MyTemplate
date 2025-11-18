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
            case CurrencyType.Energy: txtEnergy.text = amount.ToString();
                break;
            case CurrencyType.Jade: txtJade.text = amount.ToString();
                break;
            case CurrencyType.Coin: txtCoin.text = amount.ToString();
                break;
            default: break;
        }
    }
}
