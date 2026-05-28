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
            string rawUseDes = LocalizationManager.Instance.GetLocalizedValue(itemConfig.UseDescription);

            if(itemConfig.Type == ItemType.Exp)
            {
                str = string.Format(rawUseDes, itemConfig.Exp.Value);
            }
            else if (itemConfig.Type == ItemType.Food && itemConfig.Food != null && itemConfig.Food.Effects != null && itemConfig.Food.Effects.Count > 0)
            {
                var effect = itemConfig.Food.Effects[0];
                try
                {
                    str = string.Format(rawUseDes, effect.Value.ToString("F0"));
                }
                catch (System.FormatException)
                {
                    str = rawUseDes;
                }
            }
            else
            {
                str = rawUseDes;
            }

            txtUseful.text = str;
            icon.sprite = itemConfig.Icon;
            txtName.text = (itemConfig.Type == ItemType.Shard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " ") : "") + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
            txtOwned.text = item != null ? item.Quantity.ToString() : "0";
            
            if (btnUse != null)
            {
                btnUse.gameObject.SetActive(itemConfig.Type == ItemType.Food && item != null && item.Quantity > 0);
            }
            
            txtDes.text = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Description);


            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtUseful.rectTransform);

            // Force rebuild UI layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }
    }
}
