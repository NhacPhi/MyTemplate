using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;

[Serializable]
public class ShopBuyPopupProperties : WindowProperties
{
    public readonly ShopProductConfig Config;
    public readonly Sprite ItemSprite;
    public readonly Sprite ItemBg;
    public readonly Rare ItemRare;
    public readonly bool IsShard;
    public readonly string ItemName;
    public readonly string ItemDesc;
    public readonly Sprite CurrencySprite;
    public readonly int CurrentPurchase;
    public readonly int CurrentOwned;
    public readonly Action<ShopProductConfig, int> ConfirmBuyAction;

    public ShopBuyPopupProperties(
        ShopProductConfig config,
        Sprite itemSprite,
        Sprite itemBg,
        Rare itemRare,
        bool isShard,
        string itemName,
        string itemDesc,
        Sprite currencySprite,
        int currentPurchase,
        int currentOwned,
        Action<ShopProductConfig, int> confirmBuyAction)
    {
        Config = config;
        ItemSprite = itemSprite;
        ItemBg = itemBg;
        ItemRare = itemRare;
        IsShard = isShard;
        ItemName = itemName;
        ItemDesc = itemDesc;
        CurrencySprite = currencySprite;
        CurrentPurchase = currentPurchase;
        CurrentOwned = currentOwned;
        ConfirmBuyAction = confirmBuyAction;
    }
}

public class ShopBuyPopupController : WindowController<ShopBuyPopupProperties>
{
    [Header("Item Info")]
    [SerializeField] private ItemUI itemUI;
    [SerializeField] private TextMeshProUGUI txtItemName;
    [SerializeField] private TextMeshProUGUI txtItemDesc;
    [SerializeField] private TextMeshProUGUI txtCurrentOwned;

    [Header("Purchase Info")]
    [SerializeField] private TextMeshProUGUI txtPurchaseLimit;
    [SerializeField] private TextMeshProUGUI txtUnitPrice;
    [SerializeField] private Image imgUnitCurrency;
    [SerializeField] private TextMeshProUGUI txtQuantity;
    [SerializeField] private TextMeshProUGUI txtTotalPrice;
    [SerializeField] private Image imgCurrency;

    [Header("Buttons")]
    [SerializeField] private Button btnAdd1;
    [SerializeField] private Button btnSub1;
    [SerializeField] private Button btnAdd10;
    [SerializeField] private Button btnSub10;
    [SerializeField] private Button btnBuy;
    [SerializeField] private Button btnClose;

    [Header("Button Colors")]
    [SerializeField] private Color activeColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    [SerializeField] private Color inactiveColor = Color.gray;

    private int currentSelectedQuantity = 1;
    private int maxAllowedQuantity = 999;
    
    protected override void AddListeners()
    {
        base.AddListeners();
        
        if (btnAdd1 != null) { btnAdd1.onClick.RemoveAllListeners(); btnAdd1.onClick.AddListener(() => AddQuantity(1)); }
        if (btnSub1 != null) { btnSub1.onClick.RemoveAllListeners(); btnSub1.onClick.AddListener(() => AddQuantity(-1)); }
        if (btnAdd10 != null) { btnAdd10.onClick.RemoveAllListeners(); btnAdd10.onClick.AddListener(() => AddQuantity(10)); }
        if (btnSub10 != null) { btnSub10.onClick.RemoveAllListeners(); btnSub10.onClick.AddListener(() => AddQuantity(-10)); }
        
        if (btnBuy != null) { btnBuy.onClick.RemoveAllListeners(); btnBuy.onClick.AddListener(OnBtnBuyClicked); }
        if (btnClose != null) { btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(UI_Close); }
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();
        if (btnAdd1 != null) btnAdd1.onClick.RemoveAllListeners();
        if (btnSub1 != null) btnSub1.onClick.RemoveAllListeners();
        if (btnAdd10 != null) btnAdd10.onClick.RemoveAllListeners();
        if (btnSub10 != null) btnSub10.onClick.RemoveAllListeners();
        
        if (btnBuy != null) btnBuy.onClick.RemoveAllListeners();
        if (btnClose != null) btnClose.onClick.RemoveAllListeners();
    }
    
    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        
        currentSelectedQuantity = 1;
        
        if (Properties.Config.LimitCount > 0)
        {
            maxAllowedQuantity = Properties.Config.LimitCount - Properties.CurrentPurchase;
            if (maxAllowedQuantity < 1) maxAllowedQuantity = 1; // Sanity check if already maxed
        }
        else
        {
            maxAllowedQuantity = 999;
        }
        
        if (itemUI != null)
        {
            itemUI.Init(Properties.Config.ReferenceID, Properties.ItemRare, Properties.ItemSprite, Properties.ItemBg, Properties.Config.ItemAmount);
            itemUI.ActiveFragIcon(Properties.IsShard);
        }
        
        if (txtItemName != null) txtItemName.text = Properties.ItemName;
        if (txtItemDesc != null) txtItemDesc.text = Properties.ItemDesc;
        
        if (txtCurrentOwned != null)
        {
            // Try get localized string, fallback to default
            string format = LocalizationManager.Instance.GetLocalizedValue("STR_OWNED");
            if (string.IsNullOrEmpty(format)) format = "Owned: {0}";
            txtCurrentOwned.text = string.Format(format, Properties.CurrentOwned);
        }
        
        if (txtPurchaseLimit != null)
        {
            if (Properties.Config.LimitCount > 0)
            {
                txtPurchaseLimit.gameObject.SetActive(true);
                string format = LocalizationManager.Instance.GetLocalizedValue("STR_NUMBER_OF_PURCHASE");
                if (string.IsNullOrEmpty(format)) format = "{0}/{1}";
                txtPurchaseLimit.text = string.Format(format, Properties.CurrentPurchase, Properties.Config.LimitCount);
            }
            else
            {
                txtPurchaseLimit.gameObject.SetActive(false);
            }
        }
        
        if (imgCurrency != null && Properties.CurrencySprite != null)
        {
            imgCurrency.sprite = Properties.CurrencySprite;
            imgCurrency.gameObject.SetActive(true);
        }
        
        if (txtUnitPrice != null)
        {
            txtUnitPrice.text = Properties.Config.Price.ToString();
        }
        
        if (imgUnitCurrency != null && Properties.CurrencySprite != null)
        {
            imgUnitCurrency.sprite = Properties.CurrencySprite;
            imgUnitCurrency.gameObject.SetActive(true);
        }
        
        UpdateUI();
    }
    
    private void AddQuantity(int amount)
    {
        currentSelectedQuantity += amount;
        currentSelectedQuantity = Mathf.Clamp(currentSelectedQuantity, 1, maxAllowedQuantity);
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (txtQuantity != null) txtQuantity.text = currentSelectedQuantity.ToString();
        
        float totalPrice = Properties.Config.Price * currentSelectedQuantity;
        if (txtTotalPrice != null) txtTotalPrice.text = totalPrice.ToString();
        
        UpdateButtonState(btnSub1, currentSelectedQuantity > 1);
        UpdateButtonState(btnSub10, currentSelectedQuantity > 1);
        
        UpdateButtonState(btnAdd1, currentSelectedQuantity < maxAllowedQuantity);
        UpdateButtonState(btnAdd10, currentSelectedQuantity < maxAllowedQuantity);
    }
    
    private void UpdateButtonState(Button btn, bool interactable)
    {
        if (btn == null) return;
        btn.interactable = interactable;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = interactable ? activeColor : inactiveColor;
        }
    }
    
    private void OnBtnBuyClicked()
    {
        Properties.ConfirmBuyAction?.Invoke(Properties.Config, currentSelectedQuantity);
        UI_Close();
    }
}
