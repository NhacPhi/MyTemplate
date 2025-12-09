using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
//using Tech.Logger;
using Tech.Singleton;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesManager : SingletonPersistent<AddressablesManager>
{
    private readonly Dictionary<object, AsyncOperationHandle> _dicAsset = new();
    private static AddressablesManager[] a;

    protected override void Awake()
    {
        base.Awake();
        Addressables.InitializeAsync();
    }
    public async UniTask<T> LoadAssetAsync<T>(object key, Action onFailed = null, CancellationToken token = default) where T : class
    {
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        if (_dicAsset.TryGetValue(key, out var value))
        {
            return value.Result as T;
        }
        try
        {
            AsyncOperationHandle<T> opHandle;

            if (key is IEnumerable enumerable)
            {
                opHandle = Addressables.LoadAssetAsync<T>(enumerable);
            }
            else
            {
                opHandle = Addressables.LoadAssetAsync<T>(key);
            }

            await opHandle.ToUniTask(cancellationToken: token);

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _dicAsset.Add(key, opHandle);
                return (T)opHandle.Result;
            }
        }
        catch (KeyNotFoundException)
        {

        }

        //LogCommon.LogWarning($"Load Asset Failed: {key}");
        onFailed?.Invoke();
        return default;
    }
    public async UniTask<List<T>> LoadAssetsAsync<T>(object key, Action onFailed = null, CancellationToken token = default)
    {
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        if (_dicAsset.TryGetValue(key, out var value))
        {
            return value.Result as List<T>;
        }
        try
        {
            var opHandle = Addressables.LoadAssetsAsync<T>(key, null);

            await opHandle.ToUniTask(cancellationToken: token);

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _dicAsset.Add(key, opHandle);
                return (List<T>)opHandle.Result;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        //LogCommon.LogWarning($"Load Asset Failed: {key}");
        onFailed?.Invoke();
        return default;
    }
    public void RemoveAsset(object key)
    {
        if (!_dicAsset.TryGetValue(key, out var value)) return;
        Addressables.ReleaseInstance(value);
        _dicAsset.Remove(key);
    }
    public bool TryGetAssetInCache<T>(string key, out T result) where T : class
    {
        if (_dicAsset.TryGetValue(key, out var opHandle))
        {
            result = opHandle.Result as T;
            return true;
        }
        result = default;
        return false;
    }

    public async UniTask<GameObject> InstantiateAsync(object key, Transform parent = null,
        bool worldSpace = true, Action<GameObject> onComplete = null, CancellationToken token = default)
    {
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        var opHandle = Addressables.InstantiateAsync(key, parent, worldSpace);

        if (onComplete != null)
        {
            opHandle.Completed += (handle) => onComplete?.Invoke(handle.Result);
        }

        await opHandle.ToUniTask(cancellationToken: token);

        _dicAsset[key] = opHandle;
        return opHandle.Result;
    }

    public AsyncOperationHandle<GameObject> OriginInstantiateAsync(object key, Transform parent = null,
        bool worldSpace = false, CancellationToken token = default)
    {
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        var opHandle = Addressables.InstantiateAsync(key, parent, worldSpace);
        _dicAsset[key] = opHandle;
        opHandle.WithCancellation(token);
        return opHandle;
    }
}
