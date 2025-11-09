using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UIFramework;
using VContainer;

public class CharacterScene : WindowController
{
    [SerializeField] private Button btnExit;

    [SerializeField] private CharacterUI characterUI;
    [Inject] private UIManager ui;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    private void Start()
    {
        btnExit.onClick.AddListener(() =>
        {
            ui.CloseWindowScene();
        });

        characterUI = GetComponent<CharacterUI>();
        if(characterUI != null )
        {
            characterUI.Init();
        }
    }
}
