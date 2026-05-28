using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShopBundlePanel : ShopPanelBase
{
    [Header("Bundle Specific")]
    [SerializeField] protected ShopBundleUI bundlePrefab;

    protected List<ShopBundleUI> activeBundles = new List<ShopBundleUI>();

    public override void RefreshProducts()
    {
        // 1. Dọn dẹp danh sách cũ
        foreach (var bundle in activeBundles)
        {
            if (bundle != null && bundle.gameObject != null) Destroy(bundle.gameObject);
        }
        activeBundles.Clear();

        if (bundlePrefab == null || productContainer == null) return;

        // 2. Lấy cấu hình category hiện tại
        var allConfigs = gameDataBase.GetAllShopConfigs().Values.ToList();
        var categoryConfigs = allConfigs.Where(c => 
            c.ShopCategory.Equals(categoryId, StringComparison.OrdinalIgnoreCase) && 
            (string.IsNullOrEmpty(currentSubCategory) || c.SubCategory == currentSubCategory) &&
            c.IsActive
        ).OrderBy(c => c.SortOrder).ToList();

        // 3. Sinh giao diện cho từng Bundle
        foreach (var config in categoryConfigs)
        {
            if (config.SellType != ShopSellType.Bundle) continue; // Chỉ xử lý Bundle

            var bundleUI = Instantiate(bundlePrefab, productContainer);
            
            // Xử lý thông tin hiển thị chung
            
            Sprite currencySprite = null; 
            var currencyIconConfig = currencyIcons.FirstOrDefault(c => c.currencyType.Equals(config.CurrencyType, StringComparison.OrdinalIgnoreCase));
            if (currencyIconConfig != null) currencySprite = currencyIconConfig.icon;

            int currentPurchase = 0;
            if (saveSystem != null && saveSystem.Player != null && saveSystem.Player.Shop != null)
            {
                currentPurchase = saveSystem.Player.Shop.GetRecord(config.ProductID).PurchaseCount;
            }

            // Xử lý thông tin chi tiết từng Item bên trong Bundle
            List<BundleContentUIInfo> contentUIInfos = new List<BundleContentUIInfo>();
            if (config.BundleContents != null)
            {
                foreach (var content in config.BundleContents)
                {
                    var itemConfig = gameDataBase.GetItemConfig(content.ItemID);
                    if (itemConfig != null)
                    {
                        contentUIInfos.Add(new BundleContentUIInfo
                        {
                            ItemID = content.ItemID,
                            Amount = content.Amount,
                            ItemRare = itemConfig.Rarity,
                            ItemSprite = itemConfig.Icon,
                            ItemBackground = gameDataBase.GetBGItemByRare(itemConfig.Rarity),
                            IsShard = itemConfig.Type == ItemType.Shard
                        });
                    }
                }
            }

            // Setup
            bundleUI.Setup(config, contentUIInfos, currencySprite, OnBuyProduct);
            bundleUI.SetupLayout(contentUIInfos.Count);
            
            bundleUI.SetPurchaseLimit(currentPurchase, config.LimitCount);
            
            bool isSoldOut = config.LimitCount > 0 && currentPurchase >= config.LimitCount;
            bundleUI.SetSoldOutStatus(isSoldOut);

            activeBundles.Add(bundleUI);
        }
    }
}
