using deVoid.Utils;
using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Tech.Json;

public class UIManager : MonoBehaviour
{
    [SerializeField]private UISettings defaultUISettings = null;

    private UIFrame uiFrame;

    private string currentWindow;

    private void Awake()
    {
        //Signals.Get<StartSceneSignal>().AddListener(OnStartDemo);
        uiFrame = defaultUISettings.CreateUIInstance();
    }

    private async UniTask Init(CancellationToken token)
    {
        //var uiSettings = await AddressablesManager.Instance.LoadAssetAsync<UISettings>(
        //       AddressConstant.UISetting, token: token);

        //defaultUISettings = uiSettings;

        //AddressablesManager.Instance.RemoveAsset(AddressConstant.UISetting);
    }

    private void Start()
    {
        //OpenCurrentScene(ScreenIds.LoadingScene);
    }

    public void OpenCurrentScene(string id)
    {
        currentWindow = id;
        uiFrame.OpenWindow(currentWindow);
    }

    public void CloseCurrentScene()
    {
        uiFrame.CloseWindow(currentWindow);
    }
}
