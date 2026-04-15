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
    [SerializeField] private Button btnPartySetup;

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
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.InventoryScene);
        });

        btnMap.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.MapScene);
        });

        btnCharacter.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.CharacterScene);
        });

        btnPartySetup.onClick.AddListener(() =>
        {
            //uiManager.HidePanel(ScreenIds.GamePlayPanel);
            uiManager.OpenWindowScene(ScreenIds.PartySetupScene);
            UIEvent.OnPrepareBattleData?.Invoke();
        });

        btnAttack.onClick.AddListener(() => { GameEvent.OnPlayerAttack?.Invoke(); });
        btnCatchSkill.onClick.AddListener(() => { GameEvent.OnPlayerTransform?.Invoke(); });
    }

}
