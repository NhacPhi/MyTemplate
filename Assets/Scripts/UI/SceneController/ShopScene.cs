using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ShopScene : WindowController
{
    [SerializeField] private Button btnClose;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;
    
    [SerializeField] private ShopPanel shopPanel;

    private void OnEnable()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClose);
        }
        
        if (shopPanel != null)
        {
            shopPanel.Init();
        }
    }

    private void OnDisable()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
        }
    }

    private void OnClose()
    {
        uiManager.CloseWindowScene(ScreenIds.ShopScene);
    }
}
