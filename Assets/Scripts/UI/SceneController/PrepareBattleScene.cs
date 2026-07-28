using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class PrepareBattleScene : WindowController
{
    [Inject] private UIManager uiManager;
    [Inject] private BattleSessionContext _sessionContext;
    [Inject] private CurrencyManager _currencyManager;

    [SerializeField] private PartySetupControllerUI _partySetupController;
    [SerializeField] private LoadEventChannelSO _loadLocation = default;
    [SerializeField] private GameSceneSO _battleSceneSO;

    [Header("Buttons")]
    [SerializeField] private Button _btnStartBattle;

    public void OnClose()
    {
        UI_Close();
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (_btnStartBattle != null)
        {
            _btnStartBattle.onClick.AddListener(StartBattle);
        }
    }

    public void StartBattle()
    {
        // 1. Kiểm tra và trừ 5 Thể lực (Stamina / Energy)
        if (_currencyManager != null)
        {
            if (!_currencyManager.Spend(CurrencyType.Energy, 5))
            {
                Debug.LogWarning("[PrepareBattleScene] Không đủ 5 Thể lực (Stamina/Energy) để bắt đầu trận đấu!");
                return;
            }
        }

        // 2. Lưu thông tin đội hình
        if (_partySetupController != null)
        {
            _partySetupController.SavePartySetup();
        }

        // 3. Tải scene trận đấu
        ExecuteLoadBattleScene();
    }

    public void LoadBattleScene()
    {
        StartBattle();
    }

    private async void ExecuteLoadBattleScene()
    {
        if (_loadLocation != null && _battleSceneSO != null)
        {
            _loadLocation.RaiseEvent(_battleSceneSO, true);

            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            System.Action onSceneReady = () => tcs.TrySetResult();
            GameEvent.OnSceneReady += onSceneReady;
            await tcs.Task;
            GameEvent.OnSceneReady -= onSceneReady;

            UIEvent.OnToggleGamePlayScene?.Invoke(false);
            if (uiManager != null)
            {
                uiManager.OpenWindowScene(ScreenIds.BattleUIScene);
            }
        }
        else
        {
            Debug.LogError("[PrepareBattleScene] _loadLocation hoặc _battleSceneSO chưa được gán trong Inspector.");
        }
    }
}
