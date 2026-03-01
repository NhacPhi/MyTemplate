using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Protagonist playerInstance = Instantiate(_playerPrefabs, _locationEntrance.position, _locationEntrance.rotation);

        _playerTransformAnchor.Provide(playerInstance.transform);

        GameEvent.OnPlayerSpawned?.Invoke();
    }
}
