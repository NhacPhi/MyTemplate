using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class QuestScene : WindowController
{
    [SerializeField] private Button btnClose;

    [Inject] private UIManager uiManager;

    private void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClose);
        }
    }

    private void OnClose()
    {
        uiManager.CloseWindowScene(ScreenIds.QuestScene);
    }
}
