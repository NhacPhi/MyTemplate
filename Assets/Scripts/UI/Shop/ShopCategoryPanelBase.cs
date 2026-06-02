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

public class ShopCategoryPanelBase : ShopPanelBase
{
    [Header("Product UI (Single Items)")]
    [SerializeField] protected ShopProductUI productPrefab;

    protected List<ShopProductUI> activeProducts = new List<ShopProductUI>();

    public override void RefreshProducts()
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
            if (config.SellType != ShopSellType.SingleItem) continue; // Chỉ xử lý SingleItem

            var productUI = Instantiate(productPrefab, productContainer);
            
            Sprite itemSprite = null;
            Sprite itemBg = null;
            Rare itemRare = Rare.Common;
            string itemName = config.ReferenceID;
            bool isShard = false;
            
            var itemConfig = gameDataBase.GetItemConfig(config.ReferenceID);
            if (itemConfig != null)
            {
                itemSprite = itemConfig.Icon;
                itemRare = config.ItemRare ?? itemConfig.Rarity;
                itemBg = gameDataBase.GetBGItemByRare(itemRare);
                isShard = itemConfig.Type == ItemType.Shard;
                itemName = (isShard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " ") : "") + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
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
            productUI.SetShardStatus(isShard);
            
            // Cập nhật số lần mua và trạng thái Sold Out
            productUI.SetPurchaseLimit(currentPurchase, config.LimitCount);
            
            bool isSoldOut = config.LimitCount > 0 && currentPurchase >= config.LimitCount;
            productUI.SetSoldOutStatus(isSoldOut);

            activeProducts.Add(productUI);
        }
    }
}
