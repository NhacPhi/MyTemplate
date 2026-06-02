using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShopPanelBase : MonoBehaviour
{
    protected GameDataBase gameDataBase;
    protected SaveSystem saveSystem;
    protected string categoryId;
    protected UIManager uiManager;
    
    [Header("UI References")]
    [SerializeField] protected Transform productContainer;

    [Header("Currency Icons")]
    [SerializeField] protected List<CurrencyIconConfig> currencyIcons = new List<CurrencyIconConfig>();

    [Header("SubCategory (Optional)")]
    [SerializeField] protected List<ShopSubCategoryUIConfig> subCategoryUIConfigs = new List<ShopSubCategoryUIConfig>();
    
    protected string currentSubCategory;

    public virtual void Init(GameDataBase db, SaveSystem save, string catId, UIManager uiManager)
    {
        gameDataBase = db;
        saveSystem = save;
        categoryId = catId;
        this.uiManager = uiManager;
        
        InitializeSubCategories();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        
        // Reset sub tab về vị trí đầu tiên mỗi khi panel được show ra
        if (subCategoryUIConfigs.Count > 0 && subCategoryUIConfigs[0].toggle != null)
        {
            // Ép tắt tất cả các sub tab khác để đảm bảo visual được reset
            for (int i = 1; i < subCategoryUIConfigs.Count; i++)
            {
                if (subCategoryUIConfigs[i].toggle != null)
                {
                    subCategoryUIConfigs[i].toggle.ActiveToggle(false);
                }
            }
            
            subCategoryUIConfigs[0].toggle.ActiveToggle(true);
            OnSubCategorySelected(subCategoryUIConfigs[0].subCategoryId);
        }
        else
        {
            currentSubCategory = string.Empty;
            RefreshProducts();
        }
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
    }

    protected virtual void OnSubCategorySelected(string subCategoryId)
    {
        currentSubCategory = subCategoryId;
        RefreshProducts();
    }

    public abstract void RefreshProducts();

    protected virtual void OnBuyProduct(ShopProductConfig config)
    {
        // For bundle, you can override this in ShopBundlePanel and call ExecuteBuyProduct directly
        if (config.SellType == ShopSellType.Bundle)
        {
            ExecuteBuyProduct(config, 1);
            return;
        }

        Sprite itemSprite = null;
        Sprite itemBg = null;
        Rare itemRare = Rare.Common;
        string itemName = config.ReferenceID;
        string itemDesc = "";
        bool isShard = false;
        
        var itemConfig = gameDataBase.GetItemConfig(config.ReferenceID);
        if (itemConfig != null)
        {
            itemSprite = itemConfig.Icon;
            itemRare = config.ItemRare ?? itemConfig.Rarity;
            itemBg = gameDataBase.GetBGItemByRare(itemRare);
            isShard = itemConfig.Type == ItemType.Shard;
            string locName = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
            if (string.IsNullOrEmpty(locName)) locName = itemConfig.Name.ToString();
            itemName = (isShard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " ") : "") + locName;
            
            itemDesc = itemConfig.GetFullFormattedDescription(gameDataBase);
        }
        
        Sprite currencySprite = null; 
        var currencyIconConfig = currencyIcons.Find(c => c.currencyType.Equals(config.CurrencyType, StringComparison.OrdinalIgnoreCase));
        if (currencyIconConfig != null)
        {
            currencySprite = currencyIconConfig.icon;
        }

        int currentPurchase = 0;
        int currentOwned = 0;
        if (saveSystem != null && saveSystem.Player != null)
        {
            if (saveSystem.Player.Shop != null)
            {
                currentPurchase = saveSystem.Player.Shop.GetRecord(config.ProductID).PurchaseCount;
            }
            
            if (saveSystem.Player.Inventory != null)
            {
                if (saveSystem.Player.Inventory.Items != null)
                {
                    var item = saveSystem.Player.Inventory.GetItem(config.ReferenceID);
                    if (item != null)
                    {
                        currentOwned += item.Quantity;
                    }
                }
                
                if (saveSystem.Player.Inventory.Weapons != null)
                {
                    foreach (var w in saveSystem.Player.Inventory.Weapons)
                    {
                        if (w.TemplateID == config.ReferenceID) currentOwned++;
                    }
                }
                
                if (saveSystem.Player.Inventory.Armors != null)
                {
                    foreach (var a in saveSystem.Player.Inventory.Armors)
                    {
                        if (a.TemplateID == config.ReferenceID) currentOwned++;
                    }
                }
            }
        }

        var properties = new ShopBuyPopupProperties(
            config,
            itemSprite,
            itemBg,
            itemRare,
            isShard,
            itemName,
            itemDesc,
            currencySprite,
            currentPurchase,
            currentOwned,
            ExecuteBuyProduct
        );

        if (uiManager != null)
        {
            uiManager.ShowShopBuyPopup(properties);
        }
        else
        {
            Debug.LogError("UIManager is null in ShopPanelBase!");
        }
    }

    protected virtual void ExecuteBuyProduct(ShopProductConfig config, int quantity)
    {
        Debug.Log($"Buy Product: {config.ProductID} for {config.Price * quantity} {config.CurrencyType} - Amount: {quantity}");
        
        // TODO: Chỗ này bạn sẽ gọi trừ tiền (Jade/Coin) và add vật phẩm vào Inventory sau...

        // 1. Cập nhật số lần mua vào Profile
        if (saveSystem != null && saveSystem.Player != null && saveSystem.Player.Shop != null)
        {
            saveSystem.Player.Shop.AddPurchase(config.ProductID, quantity);
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            
            // 2. Refresh lại toàn bộ sản phẩm đang hiển thị để cập nhật text & trạng thái Sold Out
            RefreshProducts();
        }

        // 3. Hiển thị Popup Nhận Thưởng
        List<RewardItemData> rewards = new List<RewardItemData>();
        if (config.SellType == ShopSellType.SingleItem)
        {
            if (!string.IsNullOrEmpty(config.ReferenceID))
            {
                // Nếu ItemAmount trong config chưa set thì mặc định là 1
                int amountPerPurchase = config.ItemAmount > 0 ? config.ItemAmount : 1;
                rewards.Add(new RewardItemData(config.ReferenceID, amountPerPurchase * quantity));
            }
        }
        else if (config.SellType == ShopSellType.Bundle && config.BundleContents != null)
        {
            foreach (var content in config.BundleContents)
            {
                if (!string.IsNullOrEmpty(content.ItemID))
                {
                    rewards.Add(new RewardItemData(content.ItemID, content.Amount * quantity));
                }
            }
        }

        if (rewards.Count > 0 && uiManager != null)
        {
            uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(rewards));
        }
    }
}
