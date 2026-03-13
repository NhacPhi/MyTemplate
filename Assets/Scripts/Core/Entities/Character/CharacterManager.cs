using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Tech.Logger;
using Tech.Pool;

public class CharacterManager
{

    [Inject] private SaveSystem _saveSystem;
    [Inject] private IObjectResolver _objectResolver;

    private Dictionary<string, Entity> _characterPrefabsCache = new Dictionary<string, Entity>();
    public async UniTask<Dictionary<string, Entity>> LoadAndSpawnCharactersAsync(CancellationToken cancellation = default)
    {
        var activeSlots = _saveSystem.Player.ActiveSlots;

        var characterUIPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>("CharacterUI", token: cancellation);

        foreach (var characterConfig in activeSlots)
        {
            if(characterConfig.CharacterID != "")
            {
                var key = characterConfig.CharacterID;

                if (_characterPrefabsCache.ContainsKey(key)) continue; // Check trùng lặp

                var prefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(key, token: cancellation);

                if (prefab != null)
                {
                    _characterPrefabsCache.Add(key, prefab.GetComponent<Entity>());
                }
            }          
        }

        Dictionary<string, Entity> spawnedCharacters = new Dictionary<string, Entity>();

        foreach (var kvp in _characterPrefabsCache)
        {
            var prefabEntity = kvp.Value;

            var characterInstance = _objectResolver.Instantiate(prefabEntity, prefabEntity.transform.position, Quaternion.identity);

            var characterUI = _objectResolver.Instantiate(characterUIPrefab, prefabEntity.transform.position, Quaternion.identity, characterInstance.transform);

            //characterUI.transform.SetParent(characterInstance.transform);

            RectTransform rect = characterUI.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localScale = new Vector3(2f, 2f, 2f);

                rect.localPosition = new Vector3(0f, 2.75f, 0f);
            }

            await characterInstance.gameObject.GetComponent<EntitySkill>().InitializeAsync(token: cancellation);

            spawnedCharacters.Add(kvp.Key, characterInstance);
        }

        LogCommon.Log($"[CharacterManager] Load done {spawnedCharacters.Count} characters for this battle");

        return spawnedCharacters;
    }

    public void ClearCache()
    {
        foreach (var item in _characterPrefabsCache.Values)
        {
            AddressablesManager.Instance.RemoveAsset(item);
        }

        _characterPrefabsCache.Clear();

        LogCommon.Log("[CharacterManager] Cache cleared and assets released");
    }
}
