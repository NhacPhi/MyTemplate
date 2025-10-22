using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnPlayerInfo;

    [SerializeField] private Button btnInventory;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private void Start()
    {
        btnPlayerInfo.onClick.AddListener(() =>
        {
            uiManager.CloseAllWindows();
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        });

        currencyMM.UpdateCurrency();

        btnInventory.onClick.AddListener(() =>
        {
            uiManager.HidePanel();
            uiManager.OpenWindowScene(ScreenIds.InventoryScene);
        });
    }

}
