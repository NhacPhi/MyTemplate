using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        // 1. KIỂM TRA CACHE TRƯỚC
        if (_dicAsset.TryGetValue(key, out var existingHandle))
        {
            // Nếu Asset đang được tải dở bởi một chỗ khác, ta chỉ việc đứng chờ chung
            if (!existingHandle.IsDone)
            {
                await existingHandle.ToUniTask(cancellationToken: token);
            }
            return existingHandle.Result as T;
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

            // 2. THÊM VÀO DICTIONARY NGAY LẬP TỨC TRƯỚC KHI AWAIT
            // Để đánh dấu là "Tôi đang xử lý Key này rồi, ai đến sau thì chờ nhé"
            _dicAsset.Add(key, opHandle);

            await opHandle.ToUniTask(cancellationToken: token);

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return (T)opHandle.Result;
            }
            else
            {
                // Nếu tải lỗi, phải dọn dẹp để lần sau còn thử lại được
                _dicAsset.Remove(key);
            }
        }
        catch (Exception)
        {
            _dicAsset.Remove(key);
        }

        onFailed?.Invoke();
        return default;
    }

    public async UniTask<List<T>> LoadAssetsAsync<T>(object key, Action onFailed = null, CancellationToken token = default)
    {
        if (token == default)
        {
            token = this.GetCancellationTokenOnDestroy();
        }

        // Áp dụng logic an toàn tương tự như LoadAssetAsync
        if (_dicAsset.TryGetValue(key, out var existingHandle))
        {
            if (!existingHandle.IsDone)
            {
                await existingHandle.ToUniTask(cancellationToken: token);
            }
            return existingHandle.Result as List<T>;
        }

        try
        {
            var opHandle = Addressables.LoadAssetsAsync<T>(key, null);

            // Đánh dấu vào Cache ngay
            _dicAsset.Add(key, opHandle);

            await opHandle.ToUniTask(cancellationToken: token);

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return (List<T>)opHandle.Result;
            }
            else
            {
                _dicAsset.Remove(key);
            }
        }
        catch (Exception)
        {
            _dicAsset.Remove(key);
        }

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

        // Lưu ý: InstantiateAsync sinh ra Instance. Việc gán đè key như thế này 
        // có thể ghi đè lên Prefab gốc trong Cache nếu dùng chung Key.
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