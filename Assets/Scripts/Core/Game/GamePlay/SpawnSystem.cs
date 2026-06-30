using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private Protagonist _playerPrefabs = default;

    [SerializeField] private TransformAnchor _playerTransformAnchor = default;

    [SerializeField] private Transform _locationEntrance;

    private void OnEnable()
    {
        GameEvent.OnSceneReady += SpawnPlayer;
    }

    private void OnDisable()
    {
        GameEvent.OnSceneReady -= SpawnPlayer;

        _playerTransformAnchor.Unset();
    }
    private void SpawnPlayer()
    {
        Vector3 spawnPos = _locationEntrance.position;
        Quaternion spawnRot = _locationEntrance.rotation;

        var rootScope = FindObjectOfType<RootScope>();
        if (rootScope != null)
        {
            var sessionContext = rootScope.Container.Resolve<BattleSessionContext>();
            if (sessionContext != null && sessionContext.ReturnPosition.HasValue)
            {
                spawnPos = sessionContext.ReturnPosition.Value;
                sessionContext.ReturnPosition = null;
            }
        }

        Protagonist playerInstance = Instantiate(_playerPrefabs, spawnPos, spawnRot);

        _playerTransformAnchor.Provide(playerInstance.transform);

        GameEvent.OnPlayerSpawned?.Invoke();
    }
}
