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

    private void OnEnable()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClose);
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
