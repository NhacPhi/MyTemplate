using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Tech.Json;
using UnityEngine;

namespace Core.EventSystem
{
    public class EvenDataBase
    {
        private Dictionary<EventType, List<EventBase>> _eventDict;

        public async UniTask Init(CancellationToken token)
        {
            var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(
               AddressConstant.EventData, token: token);

            _eventDict = Json.DeserializeObject<Dictionary<EventType, List<EventBase>>>(textAsset.text);

            AddressablesManager.Instance.RemoveAsset(AddressConstant.EventData);
        }
    }

}
