using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Tech.Singleton;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class AddressablesManager : SingletonPersistent<AddressablesManager>
{
    private readonly Dictionary<object, AsyncOperationHandle> _dicAsset = new();
    
    protected override void Awake()
    {
        base.Awake();
        Addressables.InitializeAsync();
    }

    public async UniTask<T> LoadAssetAsync<T>(object key, Action onFailed = null, CancellationToken token = default) where T : class
    {
        if(token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        if(_dicAsset.TryGetValue(key, out var value))
        {
            return value.Result as T;
        }

        try
        {
            AsyncOperationHandle<T> opHandle;

            if(key is IEnumerable enumerable)
            {
                opHandle = Addressables.LoadAssetAsync<T>(enumerable);
            }
            else
            {
                opHandle = Addressables.LoadAssetAsync<T>(key);
            }
        }
        catch(KeyNotFoundException ex)
        {

        }

        onFailed?.Invoke();
        return default;
    }

    public void RemoveAsset(object key)
    {
        if (!_dicAsset.TryGetValue((object)key, out var value)) return;

        Addressables.ReleaseInstance(value);
        _dicAsset.Remove(key);
    }
}
