using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Tech.Json
{
    public class DataService
    {
        private readonly Dictionary<string, object> _dataCache = new();
        public async UniTask<T> LoadDataAsync<T>(string addressableKey,
            CancellationToken token = default, Action<T> onComplete = null) where T : class
        {
            if(_dataCache.TryGetValue(addressableKey, out var existData))
            {
                return existData as T;
            }

            await UniTask.WaitUntil(() => AddressablesManager.Instance, cancellationToken: token);

            return await LoadWithAddressable<T>(addressableKey, token, onComplete);
        }

        private async UniTask<T> LoadWithAddressable<T>(string addressableKey, CancellationToken token = default, Action<T> onComplete = default) where T : class
        {
            var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(addressableKey, token: token);
            var data = Json.DeserializeObject<T>(textAsset.text);
            onComplete?.Invoke(data);
            _dataCache[addressableKey] = data; // Cached data

            //Remove TextAsset on Data Load done
            AddressablesManager.Instance.RemoveAsset(addressableKey);

            return data;
        }

        public void ReleaseData(string key)
        {
            _dataCache.Remove(key);
        }

        public bool TryGetData<T>(string key, out T data) where T : class
        {
            var result = _dataCache.TryGetValue(key, out var existData);
            data = existData as T;
            return result;
        }

        public bool HasKey(string key)
        {
            return _dataCache.ContainsKey(key);
        }
     }
}
