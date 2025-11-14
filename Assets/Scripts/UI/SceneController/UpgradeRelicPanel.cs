using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class UpgradeRelicPanel : PanelController
{
    [SerializeField] private Button btnExit;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;
    // Start is called before the first frame update
    void Start()
    {
        btnExit.onClick.AddListener(() =>
        {
            uiManager.HidePanel();
        });
        currencyMM.UpdateCurrency();
    }

  
}
