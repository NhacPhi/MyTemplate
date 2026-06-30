using UnityEngine;
using VContainer;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string _battleID;

    [SerializeField] private LoadEventChannelSO _loadLocation = default;

    [SerializeField] private GameSceneSO _battleSceneSO;

    [Inject] BattleSessionContext _sessionContext;
    [Inject] UIManager _uiManager;
    [Inject] SceneLoader _sceneLoader;


    public void OpenPrepareScene()
    {
        _sessionContext.PendingBattleID = _battleID;
        _sessionContext.PreviousLocation = _sceneLoader.CurrentLoadedScene;
        
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _sessionContext.ReturnPosition = playerObj.transform.position;
            if (Camera.main != null)
            {
                _sessionContext.ReturnCameraPosition = Camera.main.transform.position;
            }
        }

        _uiManager.OpenWindowScene(ScreenIds.PrepareBattleScene);
        UIEvent.OnPrepareBattleData?.Invoke();
    }
}
