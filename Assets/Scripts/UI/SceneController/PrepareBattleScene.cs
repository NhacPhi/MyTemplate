using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;
public class PrepareBattleScene : WindowController
{
    [Inject] private UIManager uiManager;
    [Inject] BattleSessionContext _sessionContext;

    [SerializeField] private LoadEventChannelSO _loadLocation = default;

    [SerializeField] private GameSceneSO _battleSceneSO;

    public void OnClose()
    {
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
    }

    public void LoadBattleScene()
    {
        //_sessionContext.PendingBattleID = _sessionContext.PendingBattleID;
        _loadLocation.RaiseEvent(_battleSceneSO, true);
        uiManager.OpenWindowScene(ScreenIds.BattleUIScene);
    }
}
