using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class InventoryScene : WindowController
{
    [SerializeField] private Button btnExit;
    [SerializeField] private InventoryUI inventory;
    [SerializeField] private InventoryToggleTap defaultToggle;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private GameDataBase gameDataBase;

    [Inject] private SaveSystem save;
    private void Start()
    {
    }

    private void OnEnable()
    {
        btnExit.onClick.AddListener(OnClose);
        
        if (inventoryManager != null && gameDataBase != null)
        {
            if (inventoryManager.IsDirty)
            {
                inventoryManager.SortAllByRare();
                inventory.Init(inventoryManager, gameDataBase);
                inventoryManager.IsDirty = false;
            }
        }

        ResetToDefaultTab();

        Invoke(nameof(DelayUpdateCurrency), 0.1f);
    }

    private void ResetToDefaultTab()
    {
        if (inventory != null)
        {
            inventory.OnShowAllItemInInventory(ItemType.Item);
        }

        // Thay vì bật ngay, ta chờ 0.05s để ToggleGroup kịp nhận diện hết các Toggle khi Scene mới bật lên
        Invoke(nameof(ForceToggleOn), 0.05f);
    }

    private void ForceToggleOn()
    {
        if (defaultToggle != null)
        {
            defaultToggle.ActiveToggle(true);
        }
    }

    private void DelayUpdateCurrency()
    {
        currencyMM?.UpdateCurrency();
    }

    private void OnDisable() 
    {
        btnExit.onClick.RemoveAllListeners();
    }

    public void OnClose()
    {
        uiManager.CloseWindowScene(ScreenIds.InventoryScene);

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
