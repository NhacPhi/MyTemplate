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
    private bool _isFirstOpen = true;

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

        if (_isFirstOpen)
        {
            ResetToDefaultTab();
            _isFirstOpen = false;
        }
    }

    private void ResetToDefaultTab()
    {
        if (inventory != null)
        {
            inventory.OnShowAllItemInInventory(ItemType.Item);
        }

        // Call it immediately, but use a Coroutine to wait until end of frame to ensure ToggleGroup is ready
        StartCoroutine(ForceToggleOnCoroutine());
    }

    private System.Collections.IEnumerator ForceToggleOnCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        if (defaultToggle != null)
        {
            defaultToggle.Toggle.SetIsOnWithoutNotify(false);
            defaultToggle.ActiveToggle(true);
        }
    }

    private void OnDisable() 
    {
        btnExit.onClick.RemoveAllListeners();
    }

    public void OnClose()
    {
        _isFirstOpen = true;
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
