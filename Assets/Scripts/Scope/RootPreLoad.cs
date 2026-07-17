using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using VContainer;
using VContainer.Unity;
using System;
using Tech.Pool;

namespace Core.Scope
{
    public class RootPreLoad : IAsyncStartable, IPreload
    {
        //[Inject] private EventManager _evnetManager;
        [Inject] private SaveSystem saveSystem;
        [Inject] private UIManager uiManager;
        [Inject] private CurrencyManager currencyMM;
        [Inject] private GameDataBase gameDataBase;
        [Inject] private PlayerCharacterManager playerCharacterManager;
        [Inject] private IAudioManager audioManager;
        //[Inject] private IObjectResolver _objectResolver;
        public bool IsDone;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            IsDone = false;

            await UniTask.WaitUntil(() => AddressablesManager.Instance && GameManager.Instance
                && PoolManager.Instance && LocalizationManager.Instance, cancellationToken: cancellation);

            // Phát nhạc nền môi trường lặp lại ngay từ lúc khởi động game
            audioManager.PlaySFXAsync("AudioDB_Environment", true, true).Forget();

            //_objectResolver.Inject(PoolManager.Instance);
            saveSystem.Init();
            saveSystem.LoadSaveDataFromDisk();
            currencyMM.Init();


            //saveSystem.Settings.SaveSetting(60,5,"VIETNAMESE");
            var tasks = new List<UniTask>()
            {
                LocalizationManager.Instance.LoadLocalizedText(saveSystem.Settings.CurrentLocalized),
            };

            await UniTask.WhenAll(tasks);
            await gameDataBase.Init(cancellation);


            playerCharacterManager.Init();
            uiManager.Init();

            if (!GameEvent.IsSceneReady)
            {
                var tcs = new UniTaskCompletionSource();
                Action onSceneReady = () => tcs.TrySetResult();
                GameEvent.OnSceneReady += onSceneReady;
                await tcs.Task;
                GameEvent.OnSceneReady -= onSceneReady;
            }

            uiManager.OpenWindowScene(ScreenIds.StartGameScene);
            uiManager.ShowPanel(ScreenIds.PanelStartGame);

            IsDone = true;
        }

        public Action OnLoadDone { get; set; }

        public bool IsLoadDone() => IsDone;

    }

}
