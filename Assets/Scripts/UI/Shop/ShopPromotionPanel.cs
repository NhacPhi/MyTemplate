using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ShopPromotionPanel : ShopPanelBase
{
    [Header("Promotion Settings")]
    [SerializeField] private ShopPromotionUI promotionPrefab;

    protected List<ShopPromotionUI> activePromotions = new List<ShopPromotionUI>();

    public override void RefreshProducts()
    {
        // 1. Dọn dẹp danh sách cũ
        foreach (var promo in activePromotions)
        {
            if (promo != null && promo.gameObject != null) Destroy(promo.gameObject);
        }
        activePromotions.Clear();

        if (promotionPrefab == null || productContainer == null) return;

        // 2. Lọc các sản phẩm thuộc category này
        var allConfigs = gameDataBase.GetAllShopConfigs().Values.ToList();
        
        var categoryConfigs = allConfigs.Where(c => 
            c.ShopCategory.Equals(categoryId, StringComparison.OrdinalIgnoreCase) && 
            (string.IsNullOrEmpty(currentSubCategory) || c.SubCategory == currentSubCategory) &&
            c.IsActive
        ).OrderBy(c => c.SortOrder).ToList();

        // 3. Tạo UI
        foreach (var config in categoryConfigs)
        {
            var promoUI = Instantiate(promotionPrefab, productContainer);
            
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

            promoUI.Setup(config, itemRare, itemSprite, itemBg, currencySprite, OnBuyProduct);
            promoUI.SetName(itemName);
            promoUI.SetShardStatus(isShard);
            
            promoUI.SetPurchaseLimit(currentPurchase, config.LimitCount);
            
            bool isSoldOut = config.LimitCount > 0 && currentPurchase >= config.LimitCount;
            promoUI.SetSoldOutStatus(isSoldOut);

            activePromotions.Add(promoUI);
        }
    }
}
