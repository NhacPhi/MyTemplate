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
        [Inject] private CharacterStatManager characterStatMM;
        [Inject] private GameNarrativeData gameNarrative;
        [Inject] private IObjectResolver _objectResolver;
        public bool IsDone;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            IsDone = false;

            await UniTask.WaitUntil(() => AddressablesManager.Instance && GameManager.Instance
                && PoolManager.Instance && LocalizationManager.Instance, cancellationToken: cancellation);
            saveSystem.Init();
            saveSystem.LoadSaveDataFromDisk();
            currencyMM.Init();
            characterStatMM.Init();
            //saveSystem.Settings.SaveSetting(60,5,"VIETNAMESE");
            var tasks = new List<UniTask>()
            {
                LocalizationManager.Instance.LoadLocalizedText(saveSystem.Settings.CurrentLocalized),
            };
            _objectResolver.Inject(PoolManager.Instance);
            await UniTask.WhenAll(tasks);
            gameNarrative.Init();
            uiManager.Init();
            uiManager.OpenWindowScene(ScreenIds.StartGameScene);
            uiManager.ShowPanel(ScreenIds.PanelStartGame);

            IsDone = true;
        }

        public Action OnLoadDone { get; set; }

        public bool IsLoadDone() => IsDone;

    }

}
