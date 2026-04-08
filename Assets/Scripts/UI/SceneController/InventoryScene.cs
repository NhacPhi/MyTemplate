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
    [Inject] private InventoryManager inventoryManager;
    [Inject] private GameDataBase gameDataBase;

    [Inject] private SaveSystem save;
    private void Start()
    {
        btnExit.onClick.AddListener(OnClose);
        currencyMM.UpdateCurrency();
        inventoryManager.SortAllByRare();
        inventory.Init(inventoryManager, gameDataBase);
    }

   public void OnClose()
    {
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
        save.SaveDataToDisk(GameSaveType.All);
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
