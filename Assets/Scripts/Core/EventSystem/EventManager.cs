using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;

public class EventManager
{
    [Inject] public UIManager UIManager { get; private set; }

    public void Init(List<UniTask> tasks, CancellationToken token = default)
    {

    }

    public void ShowLog()
    {
        Debug.Log("LOL");
    }
}
