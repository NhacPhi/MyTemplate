using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShopPanelBase : MonoBehaviour
{
    protected GameDataBase gameDataBase;
    protected SaveSystem saveSystem;
    protected string categoryId;
    
    [Header("UI References")]
    [SerializeField] protected Transform productContainer;

    [Header("Currency Icons")]
    [SerializeField] protected List<CurrencyIconConfig> currencyIcons = new List<CurrencyIconConfig>();

    [Header("SubCategory (Optional)")]
    [SerializeField] protected List<ShopSubCategoryUIConfig> subCategoryUIConfigs = new List<ShopSubCategoryUIConfig>();
    
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
