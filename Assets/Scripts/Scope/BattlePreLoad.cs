using VContainer;
using VContainer.Unity;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Core.Scope;
using Tech.Pool;

public class BattlePreLoad : IAsyncStartable, IPreload
{
    public bool IsDone;
    public async UniTask StartAsync(CancellationToken cancellation = default)
    {

    }

    public Action OnLoadDone { get; set; }

    public bool IsLoadDone() => IsDone;
}
