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
        _sessionContext.PendingBattleID = _battleID.Trim();
        // Ưu tiên 1: Tự động dò tìm GameSceneSO bằng tên Scene hiện tại qua Registry (An toàn nhất cho Prefab dùng chung)
        // Ưu tiên 2: _sceneLoader.CurrentLoadedScene (injected)
        // Ưu tiên 3: SceneLoader.Instance.CurrentLoadedScene (singleton)
        // Ưu tiên 4: SceneLoader.LastLoadedLocation (static cache fallback)
        GameSceneSO prevLoc = null;

        string currentSceneName = gameObject.scene.name;
        prevLoc = SceneLoader.GetRegisteredScene(currentSceneName);
        if (prevLoc != null)
        {
            Debug.Log($"[BattleTrigger] Tự động xác định PreviousLocation thành công qua active scene '{currentSceneName}': {prevLoc.name}");
        }

        if (prevLoc == null && _sceneLoader != null && _sceneLoader.CurrentLoadedScene != null)
        {
            prevLoc = _sceneLoader.CurrentLoadedScene;
            Debug.LogWarning($"[BattleTrigger] Falling back to injected _sceneLoader.CurrentLoadedScene: {prevLoc.name}");
        }
        else if (prevLoc == null && SceneLoader.Instance != null && SceneLoader.Instance.CurrentLoadedScene != null)
        {
            prevLoc = SceneLoader.Instance.CurrentLoadedScene;
            Debug.LogWarning($"[BattleTrigger] Falling back to SceneLoader.Instance.CurrentLoadedScene: {prevLoc.name}");
        }
        else if (prevLoc == null && SceneLoader.LastLoadedLocation != null)
        {
            prevLoc = SceneLoader.LastLoadedLocation;
            Debug.LogWarning($"[BattleTrigger] Falling back to static LastLoadedLocation: {prevLoc.name}");
        }

        if (prevLoc == null)
        {
            string registryKeys = SceneLoader.DumpRegistryKeys();
            Debug.LogError($"[BattleTrigger] KHÔNG THỂ xác định được PreviousLocation! Active Scene Name: '{currentSceneName}'. Registry Keys: {registryKeys}");
        }

        _sessionContext.PreviousLocation = prevLoc;
        _sessionContext.PreviousLocationName = prevLoc != null ? prevLoc.name : null;
        
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
