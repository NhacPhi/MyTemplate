using NPOI.OpenXmlFormats.Spreadsheet;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnPlayerInfo;

    [SerializeField] private Button btnInventory;
    [SerializeField] private Button btnCharacter;
    [SerializeField] private Button btnMap;

    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnCatchSkill;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private void Start()
    {
        btnPlayerInfo.onClick.AddListener(() =>
        {
            uiManager.CloseAllWindows();
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        });

        currencyMM.UpdateCurrency();

        btnInventory.onClick.AddListener(() =>
        {
            uiManager.HidePanel();
            uiManager.OpenWindowScene(ScreenIds.InventoryScene);
        });

        btnMap.onClick.AddListener(() =>
        {
            uiManager.HidePanel();
            uiManager.OpenWindowScene(ScreenIds.MapScene);
        });

        btnCharacter.onClick.AddListener(() =>
        {
            uiManager.HidePanel();
            uiManager.OpenWindowScene(ScreenIds.CharacterScene);
        });

        btnAttack.onClick.AddListener(() => { GameEvent.OnPlayerAttack?.Invoke(); });
        btnCatchSkill.onClick.AddListener(() => { GameEvent.OnPlayerTransform?.Invoke(); });
    }

}
