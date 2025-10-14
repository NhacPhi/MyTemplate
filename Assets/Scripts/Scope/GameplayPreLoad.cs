using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System;

namespace Core.Scope
{
    public class GameplayPreLoad : IAsyncStartable, IPreload
    {
        [Inject] private EventManager _evnetManager;
        [Inject] private SaveSystem _saveSystem;
        [Inject] private UIManager _uiManager;

        public bool IsDone;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            IsDone = false;
            _saveSystem.Init();
            _saveSystem.LoadSaveDataFromDisk();
            //_saveSystem.Settings.SaveSetting(60,5,"VIETNAMESE");
            var tasks = new List<UniTask>()
            {
                LocalizationManager.Instance.LoadLocalizedText(_saveSystem.Settings.CurrentLocalized),
            };

            await UniTask.WhenAll(tasks);

            _uiManager.OpenWindowScene(ScreenIds.StartGameScene);
            _uiManager.ShowPanel(ScreenIds.PanelStartGame);

            IsDone = true;
        }

        public Action OnLoadDone { get; set; }

        public bool IsLoadDone() => IsDone;

    }

}
