using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;


public class MapScene : WindowController
{
    [Inject] private UIManager uiManager;

    [SerializeField] private Button btnGo;
    [SerializeField] private Image imageLocation;

    [SerializeField] private LoadEventChannelSO _loadLocation = default;

    private GameSceneSO currentScene;
    private void Awake()
    {
        btnGo.onClick.AddListener(()=>
        {
            _loadLocation.RaiseEvent(currentScene, false);
        });
    }

    private void OnEnable()
    {
        UIEvent.OnSelectToggleMap += UpdateMapScene;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleMap -= UpdateMapScene;
    }
    public void OnClose()
    {
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
    }

    public void UpdateMapScene(GameSceneSO location, Sprite sprite)
    {
        imageLocation.sprite =  sprite;
        currentScene = location;
    }
}
