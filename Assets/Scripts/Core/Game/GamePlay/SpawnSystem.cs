using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private Protagonist _playerPrefabs = default;

    // Anchor that will hold the player transform after spawning
    [SerializeField] private TransformAnchor _playerTransformAnchor = default;

    // Optional explicit entrance point. If left null, the system will search for a GameObject
    // tagged "PlayerSpawn" in the loaded scene and use its transform.
    [SerializeField] private Transform _locationEntrance;


    private void OnEnable()
    {
        GameEvent.OnSceneReady += StartSpawnRoutine;
    }

    private void OnDisable()
    {
        GameEvent.OnSceneReady -= StartSpawnRoutine;
        // Ensure we clear the anchor when this system is destroyed
        _playerTransformAnchor.Unset();
    }

    private void StartSpawnRoutine()
    {
        StartCoroutine(SpawnPlayerDeferred());
    }

    private IEnumerator SpawnPlayerDeferred()
    {
        // Wait 1 frame to ensure all objects in the new scene are fully initialized,
        // injected, and active before we perform the spawn logic.
        yield return null;
        SpawnPlayer();
    }

    // Helper to locate a fallback spawn point in the scene.
    private Transform FindFallbackEntrance()
    {
        var spawnObj = GameObject.FindWithTag("PlayerSpawn");
        if (spawnObj != null)
        {
            return spawnObj.transform;
        }
        Debug.LogWarning("[SpawnSystem] No explicit entrance set and no object with tag 'PlayerSpawn' found. Using (0,0,0).");
        return null;
    }
    private void SpawnPlayer()
    {
        // Resolve the spawn position/rotation. Prefer the explicitly assigned entrance.
        Vector3 spawnPos;
        Quaternion spawnRot;
        if (_locationEntrance != null)
        {
            spawnPos = _locationEntrance.position;
            spawnRot = _locationEntrance.rotation;
        }
        else
        {
            var fallback = FindFallbackEntrance();
            if (fallback != null)
            {
                spawnPos = fallback.position;
                spawnRot = fallback.rotation;
            }
            else
            {
                // Fallback to origin if nothing is found.
                spawnPos = Vector3.zero;
                spawnRot = Quaternion.identity;
            }
        }

        // If a battle session requests a specific return position, override the above ONLY if returning to the same scene.
        var rootScope = FindObjectOfType<RootScope>();
        if (rootScope != null)
        {
            var sessionContext = rootScope.Container.Resolve<BattleSessionContext>();
            if (sessionContext != null && sessionContext.ReturnPosition.HasValue)
            {
                string currentSceneName = gameObject.scene.name;
                bool isSameScene = (sessionContext.PreviousLocation != null && sessionContext.PreviousLocation.name == currentSceneName)
                                || (!string.IsNullOrEmpty(sessionContext.PreviousLocationName) && sessionContext.PreviousLocationName == currentSceneName);

                if (isSameScene)
                {
                    spawnPos = sessionContext.ReturnPosition.Value;
                }
                else
                {
                    Debug.Log($"[SpawnSystem] Chuyển sang scene mới ('{currentSceneName}'), bỏ qua ReturnPosition của scene cũ.");
                    sessionContext.ReturnCameraPosition = null;
                }

                sessionContext.ReturnPosition = null;
            }
        }

        Protagonist playerInstance = Instantiate(_playerPrefabs, spawnPos, spawnRot);

        // Provide the spawned player's transform to the anchor so other systems can access it.
        _playerTransformAnchor.Provide(playerInstance.transform);

        GameEvent.OnPlayerSpawned?.Invoke();

        if (rootScope != null)
        {
            var sessionContext = rootScope.Container.Resolve<BattleSessionContext>();
            if (sessionContext != null)
            {
                sessionContext.ReturnCameraPosition = null;
            }
        }
    }
}
