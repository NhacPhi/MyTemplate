using NPOI.OpenXmlFormats.Spreadsheet;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnGamePanel;

    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnCatchSkill;
    [SerializeField] private Button btnMap;
    [SerializeField] private Button btnQuest;


    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    protected override void Awake()
    {
        base.Awake();
        UIEvent.OnToggleGamePlayScene += ToggleScene;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UIEvent.OnToggleGamePlayScene -= ToggleScene;
    }

    private void ToggleScene(bool show)
    {
        gameObject.SetActive(show);
    }

    private void Start()
    {
        btnGamePanel.onClick.AddListener(() =>
        {
            uiManager.CloseAllWindows();
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        });


        btnAttack.onClick.AddListener(() => { GameEvent.OnPlayerAttack?.Invoke(); });
        btnCatchSkill.onClick.AddListener(() => { GameEvent.OnPlayerTransform?.Invoke(); });
        
        btnMap.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.MapScene);
        });
        
        btnQuest.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.QuestScene);
        });
    }

    private void OnEnable()
    {
        Invoke(nameof(DelayUpdateCurrency), 0.1f);
    }

    private void DelayUpdateCurrency()
    {
        currencyMM?.UpdateCurrency();
    }
}
