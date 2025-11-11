using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using VContainer;
using VContainer.Unity;
using System;

namespace Core.Scope
{
    public class GameplayPreLoad : IAsyncStartable, IPreload
    {
        [Inject] private EventManager _evnetManager;
        [Inject] private SaveSystem saveSystem;
        [Inject] private UIManager uiManager;
        [Inject] private CurrencyManager currencyMM;
        [Inject] private CharacterStatManager characterStatMM;

        public bool IsDone;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            IsDone = false;
            saveSystem.Init();
            saveSystem.LoadSaveDataFromDisk();
            currencyMM.Init();
            characterStatMM.Init();
            //saveSystem.Settings.SaveSetting(60,5,"VIETNAMESE");
            var tasks = new List<UniTask>()
            {
                LocalizationManager.Instance.LoadLocalizedText(saveSystem.Settings.CurrentLocalized),
            };

            await UniTask.WhenAll(tasks);

            uiManager.OpenWindowScene(ScreenIds.StartGameScene);
            uiManager.ShowPanel(ScreenIds.PanelStartGame);

            IsDone = true;
        }

        public Action OnLoadDone { get; set; }

        public bool IsLoadDone() => IsDone;

    }

}
