using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnPlayerInfo;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private void Start()
    {
        btnPlayerInfo.onClick.AddListener(() =>
        {
            uiManager.CloseAllWindows();
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        });

        foreach(var currency in currencyMM.Currencies)
        {
            UIEvent.OnCurrencyChanged?.Invoke(currency.Key, currency.Value);
        }
    }
}
