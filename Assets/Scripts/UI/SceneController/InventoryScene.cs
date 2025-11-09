using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class InventoryScene : WindowController
{
    [SerializeField] private Button btnExit;
    [SerializeField] private InventoryUI inventory;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    private void Start()
    {
        btnExit.onClick.AddListener(OnClose);
        currencyMM.UpdateCurrency();
        inventory.Init(save, gameDataBase);
    }

   public void OnClose()
    {
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
    }

    public void OnLoadInventory(ItemType type)
    {
        switch(type)
        {
            case ItemType.Item:

                break;
            case ItemType.Weapon:

                break;
        }
    }
}
