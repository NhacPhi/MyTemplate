using UnityEngine;
using UIFramework;
using System.Collections.Generic;
using UnityEngine.Events;
using System;


public class PanelStartGameController : PanelController
{
    [SerializeField] private List<NavigationButton> buttons;
    // Start is called before the first frame update
    void Start()
    {
        if (buttons.Count > 0)
        {
            foreach(var button in buttons)
            {
                UnityAction action = () => { ClickButtonOnPanel(button.StringID()); };
                button.button.onClick.AddListener(action);
            }
        }
    }

    private void ClickButtonOnPanel(string id)
    {
        if (id != null)
        {
            UIEvent.OnClickNavigationButton?.Invoke(id);
            Debug.Log("Button Click");
        }

    }
}
