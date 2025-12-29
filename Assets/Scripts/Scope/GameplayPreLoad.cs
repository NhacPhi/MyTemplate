using Core.Scope;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using VContainer.Unity;
using System;

public class GameplayPreload : IAsyncStartable, IPreload
{
    [Inject] QuestManager questManager;
    public bool IsDone;
    public async UniTask StartAsync(CancellationToken cancellation = default)
    {
        questManager.StartGame();
    }

    public Action OnLoadDone { get; set; }

    public bool IsLoadDone() => IsDone;
}
