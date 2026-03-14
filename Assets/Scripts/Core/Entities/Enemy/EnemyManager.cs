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

public class EnemyManager
{
    [Inject] private BattleSessionContext _battleSession;
    [Inject] private GameDataBase _gameDataBase;
    [Inject] private IObjectResolver _objectResolver;

    private Dictionary<string, Entity> _enemyPrefabsCache = new Dictionary<string, Entity>();

    public async UniTask<List<Entity>> LoadAndSpawnEnemiesAsync(CancellationToken cancellation = default)
    {
        var battleConfig = _gameDataBase.GetBattleConfig(_battleSession.PendingBattleID);

        foreach (var enemyConfig in battleConfig.Enemies)
        {
            var key = enemyConfig.EnemyID;

            if (!_enemyPrefabsCache.ContainsKey(key))
            {
                var prefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(key, token: cancellation);

                if (prefab != null)
                {
                    _enemyPrefabsCache.Add(key, prefab.GetComponent<Entity>());
                }
                else
                {
                    LogCommon.LogError($"[EnemyManager] Không tìm thấy Prefab cho ID: {key}");
                    continue;
                }
            }
        }


        List<Entity> spawnedEnemies = new List<Entity>();

        var enemyUIPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>("CharacterUI", token: cancellation);

        var targetHitboxPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>("TargetHitbox", token: cancellation);

        foreach (var config in battleConfig.Enemies)
        {
            var prefab = _enemyPrefabsCache.GetValueOrDefault(config.EnemyID);

            if (prefab != null)
            {
                var enemyInstance = _objectResolver.Instantiate(prefab,
                       prefab.transform.position, Quaternion.identity);

                var enemyUI = _objectResolver.Instantiate(enemyUIPrefab,
                    enemyUIPrefab.transform.position, Quaternion.identity, enemyInstance.transform);

                var hitbox = _objectResolver.Instantiate(targetHitboxPrefab,
                    targetHitboxPrefab.transform.position, Quaternion.identity, enemyInstance.transform);

                //enemyUI.transform.SetParent(enemyInstance.transform);

                RectTransform rect = enemyUI.GetComponent<RectTransform>();

                if(rect != null)
                {
                    rect.localScale = new Vector3(2f, 2f, 2f);

                    rect.localPosition = new Vector3(0f, 2.75f, 0f);
                }
                spawnedEnemies.Add(enemyInstance);
            }
        }

        LogCommon.Log($"[EnemyManager] Spawn done {spawnedEnemies.Count} enemies for this battle");

        return spawnedEnemies;
    }

    public void ClearCache()
    {
        foreach(var item in _enemyPrefabsCache.Values)
        {
            AddressablesManager.Instance.RemoveAsset(item);
        }

        _enemyPrefabsCache.Clear();

        LogCommon.Log("[EnemyManager] Cache cleared and assets released");
    }
}
