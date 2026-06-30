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

    [SerializeField] private GameSceneSO currentScene;
    private void Awake()
    {
        btnGo.onClick.AddListener(()=>
        {
            UI_Close();
            _loadLocation.RaiseEvent(currentScene, true);
        });
    }

    private void OnEnable()
    {
        UIEvent.OnSelectToggleMap += UpdateMapScene;
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleMap -= UpdateMapScene;
        Time.timeScale = 1f;
    }
    public void OnClose()
    {
        UI_Close();
    }

    public void UpdateMapScene(GameSceneSO location, Sprite sprite)
    {
        imageLocation.sprite =  sprite;
        currentScene = location;
    }
}
