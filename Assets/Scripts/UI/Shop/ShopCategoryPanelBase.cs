using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class ShopSubCategoryUIConfig
{
    public string subCategoryId; 
    public ShopSubCategoryToggle toggle; // Kéo thả Sub-Tab có sẵn trên Scene vào đây
}

[Serializable]
public class CurrencyIconConfig
{
    public string currencyType; // VD: "Jade", "Coin"
    public Sprite icon;
}

public abstract class ShopCategoryPanelBase : MonoBehaviour
{
    protected GameDataBase gameDataBase;
    protected SaveSystem saveSystem;
    protected string categoryId;
    
    [Header("UI References")]
    [SerializeField] protected Transform productContainer;
    [SerializeField] protected ShopProductUI productPrefab;

    [Header("Currency Icons")]
    [SerializeField] protected List<CurrencyIconConfig> currencyIcons = new List<CurrencyIconConfig>();

    [Header("SubCategory (Optional)")]
    [SerializeField] protected List<ShopSubCategoryUIConfig> subCategoryUIConfigs = new List<ShopSubCategoryUIConfig>();

    protected List<ShopProductUI> activeProducts = new List<ShopProductUI>();
    
    protected string currentSubCategory;

    public virtual void Init(GameDataBase db, SaveSystem save, string catId)
    {
        gameDataBase = db;
        saveSystem = save;
        categoryId = catId;
        
        InitializeSubCategories();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        RefreshProducts();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    protected virtual void InitializeSubCategories()
    {
        // Setup cho các Sub Tab đã có sẵn trên Scene
        foreach (var config in subCategoryUIConfigs)
        {
            if (config.toggle != null && !string.IsNullOrEmpty(config.subCategoryId))
            {
                string subCategoryKey = $"SHOP_SUBCATEGORY_{config.subCategoryId.ToUpper()}";
                config.toggle.Setup(config.subCategoryId, subCategoryKey, OnSubCategorySelected);
            }
        }

        // Nếu có khai báo Sub Tab thì chọn cái đầu tiên, ngược lại bỏ qua
        if (subCategoryUIConfigs.Count > 0 && subCategoryUIConfigs[0].toggle != null)
        {
            subCategoryUIConfigs[0].toggle.ActiveToggle(true);
            OnSubCategorySelected(subCategoryUIConfigs[0].subCategoryId);
        }
        else
        {
            currentSubCategory = string.Empty;
            RefreshProducts();
        }
    }

    protected virtual void OnSubCategorySelected(string subCategoryId)
    {
        currentSubCategory = subCategoryId;
        RefreshProducts();
    }

    public virtual void RefreshProducts()
    {
        foreach (var product in activeProducts)
        {
            if (product != null && product.gameObject != null) Destroy(product.gameObject);
        }
        activeProducts.Clear();

        if (productPrefab == null || productContainer == null) return;

        var allConfigs = gameDataBase.GetAllShopConfigs().Values.ToList();
        
        var categoryConfigs = allConfigs.Where(c => 
            c.ShopCategory.Equals(categoryId, StringComparison.OrdinalIgnoreCase) && 
            (string.IsNullOrEmpty(currentSubCategory) || c.SubCategory == currentSubCategory) &&
            c.IsActive
        ).OrderBy(c => c.SortOrder).ToList();

        foreach (var config in categoryConfigs)
        {
            var productUI = Instantiate(productPrefab, productContainer);
            
            Sprite itemSprite = null;
            Sprite itemBg = null;
            Rare itemRare = Rare.Common;
            string itemName = config.ReferenceID;
            
            if (config.SellType == ShopSellType.SingleItem)
            {
                var itemConfig = gameDataBase.GetItemConfig(config.ReferenceID);
                if (itemConfig != null)
                {
                    itemSprite = itemConfig.Icon;
                    itemRare = itemConfig.Rarity;
                    itemBg = gameDataBase.GetBGItemByRare(itemRare);
                    itemName = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
                }
            }
            else if (config.SellType == ShopSellType.Bundle)
            {
                itemName = LocalizationManager.Instance.GetLocalizedValue($"BUNDLE_{config.ProductID}");
            }
            
            Sprite currencySprite = null; 
            var currencyIconConfig = currencyIcons.FirstOrDefault(c => c.currencyType.Equals(config.CurrencyType, StringComparison.OrdinalIgnoreCase));
            if (currencyIconConfig != null)
            {
                currencySprite = currencyIconConfig.icon;
            }

            int currentPurchase = 0;
            if (saveSystem != null && saveSystem.Player != null && saveSystem.Player.Shop != null)
            {
                currentPurchase = saveSystem.Player.Shop.GetRecord(config.ProductID).PurchaseCount;
            }

            productUI.Setup(config, itemRare, itemSprite, itemBg, currencySprite, OnBuyProduct);
            productUI.SetName(itemName);
            
            // Cập nhật số lần mua và trạng thái Sold Out
            productUI.SetPurchaseLimit(currentPurchase, config.LimitCount);
            
            bool isSoldOut = config.LimitCount > 0 && currentPurchase >= config.LimitCount;
            productUI.SetSoldOutStatus(isSoldOut);

            activeProducts.Add(productUI);
        }
    }

    protected virtual void OnBuyProduct(ShopProductConfig config)
    {
        Debug.Log($"Buy Product: {config.ProductID} for {config.Price} {config.CurrencyType}");
        
        // TODO: Chỗ này bạn sẽ gọi trừ tiền (Jade/Coin) và add vật phẩm vào Inventory sau...

        // 1. Cập nhật số lần mua vào Profile
        if (saveSystem != null && saveSystem.Player != null && saveSystem.Player.Shop != null)
        {
            saveSystem.Player.Shop.AddPurchase(config.ProductID, 1);
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            
            // 2. Refresh lại toàn bộ sản phẩm đang hiển thị để cập nhật text & trạng thái Sold Out
            RefreshProducts();
        }
    }
}
