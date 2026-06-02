using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class ItemCardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtOwned;
    [SerializeField] private TextMeshProUGUI txtUseful;
    [SerializeField] private TextMeshProUGUI txtDes;

    [SerializeField] private GameObject content;

    [SerializeField] private Button btnUse;
    [SerializeField] private Button btnResource;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;
    [Inject] private InventoryManager inventoryManager;

    private string _currentItemID;

    private void Awake()
    {
        if (btnUse != null)
        {
            btnUse.onClick.AddListener(OnBtnUseClicked);
        }
    }

    private void OnBtnUseClicked()
    {
        if (string.IsNullOrEmpty(_currentItemID)) return;
        
        bool used = inventoryManager.UseItem(_currentItemID);
        if (used)
        {
            UpdateItemCardInfor(_currentItemID);
            UIEvent.OnItemChanged?.Invoke(_currentItemID);
        }
    }

    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateItemCardInfor;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateItemCardInfor;
    }
    public void UpdateItemCardInfor(string id)
    {
        _currentItemID = id;
        ItemSaveData item = save.Player.Inventory.GetItem(id);
        var itemConfig = gameDataBase.GetItemConfig(id);
        string str = "";
        if(itemConfig != null)
        {
            txtUseful.text = itemConfig.GetFormattedUseDescription();
            icon.sprite = itemConfig.Icon;
            txtName.text = (itemConfig.Type == ItemType.Shard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " ") : "") + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
            txtOwned.text = item != null ? item.Quantity.ToString() : "0";
            
            if (btnUse != null)
            {
                btnUse.gameObject.SetActive(itemConfig.Type == ItemType.Food && item != null && item.Quantity > 0);
            }
            
            txtDes.text = itemConfig.GetFormattedDescription();


            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtUseful.rectTransform);

            // Force rebuild UI layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }
    }
}
