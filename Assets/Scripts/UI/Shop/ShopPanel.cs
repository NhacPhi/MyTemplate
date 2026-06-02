using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using System.Linq;

[Serializable]
public class ShopCategoryUIConfig
{
    public string categoryId; 
    public ShopCategoryToggle toggle; // Kéo thả Tab tương ứng có sẵn trên Scene vào đây
    public ShopPanelBase subPanel; 
}

public class ShopPanel : MonoBehaviour
{
    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem saveSystem;
    [Inject] private UIManager uiManager;

    [SerializeField] private List<ShopCategoryUIConfig> categoryUIConfigs = new List<ShopCategoryUIConfig>();

    private string currentCategory;
    private HashSet<ShopPanelBase> initializedPanels = new HashSet<ShopPanelBase>();

    public void Init()
    {
        InitializeCategories();
    }

    private void InitializeCategories()
    {
        // Setup cho các Tab đã có sẵn trên Scene
        foreach (var config in categoryUIConfigs)
        {
            if (config.toggle != null)
            {
                string categoryKey = $"SHOP_CATEGORY_{config.categoryId.ToUpper()}";
                config.toggle.Setup(config.categoryId, categoryKey, OnCategorySelected);
            }
        }

        // Tự động chọn Tab đầu tiên và tắt các tab khác
        if (categoryUIConfigs.Count > 0 && categoryUIConfigs[0].toggle != null)
        {
            for (int i = 1; i < categoryUIConfigs.Count; i++)
            {
                if (categoryUIConfigs[i].toggle != null)
                {
                    categoryUIConfigs[i].toggle.ActiveToggle(false);
                }
            }
            
            categoryUIConfigs[0].toggle.ActiveToggle(true);
            OnCategorySelected(categoryUIConfigs[0].categoryId);
        }
    }

    private void OnCategorySelected(string categoryId)
    {
        Debug.Log($"[ShopPanel] OnCategorySelected called with ID: {categoryId}");
        currentCategory = categoryId;
        
        var currentUIConfig = categoryUIConfigs.FirstOrDefault(c => c.categoryId.Equals(currentCategory, StringComparison.OrdinalIgnoreCase));
        
        foreach (var uiConfig in categoryUIConfigs)
        {
            if (uiConfig.subPanel != null)
            {
                if (uiConfig == currentUIConfig)
                {
                    // Lần đầu mở thì gọi Init
                    if (!initializedPanels.Contains(uiConfig.subPanel))
                    {
                        uiConfig.subPanel.Init(gameDataBase, saveSystem, uiConfig.categoryId, uiManager);
                        initializedPanels.Add(uiConfig.subPanel);
                    }
                    uiConfig.subPanel.Show();
                }
                else
                {
                    uiConfig.subPanel.Hide();
                }
            }
        }

        if (currentUIConfig == null || currentUIConfig.subPanel == null)
        {
            Debug.LogWarning($"Missing ShopCategoryUIConfig or SubPanel for category: {currentCategory}");
        }
    }
}
