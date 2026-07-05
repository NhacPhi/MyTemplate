using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class QuestIndicatorManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject mainQuestIndicatorPrefab;
    [SerializeField] private GameObject dailyQuestIndicatorPrefab;

    [Header("Settings")]
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 2.5f, 0);

    [Inject] private QuestManager questManager;
    [Inject] private DailyQuestManager dailyQuestManager;
    [Inject] private GameNarrativeData gameNarrativeData;

    // Track all active NPCs in the scene
    private List<StepController> activeNPCs = new List<StepController>();

    // Map NPC to their current active indicator instance
    private Dictionary<StepController, GameObject> activeIndicators = new Dictionary<StepController, GameObject>();

    private void OnEnable()
    {
        GameEvent.OnNPCSpawned += RegisterNPC;
        GameEvent.OnNPCDestroyed += UnregisterNPC;
        GameEvent.OnQuestUpdated += UpdateAllIndicators;
    }

    private void OnDisable()
    {
        GameEvent.OnNPCSpawned -= RegisterNPC;
        GameEvent.OnNPCDestroyed -= UnregisterNPC;
        GameEvent.OnQuestUpdated -= UpdateAllIndicators;
    }

    private void Start()
    {
        // VContainer will inject QuestManager. If it's missing, ensure this manager is in the DI scope.
        if (questManager == null)
        {
            Debug.LogWarning("[QuestIndicatorManager] QuestManager is not injected!");
        }
        UpdateAllIndicators();
    }



    private void RegisterNPC(StepController npc)
    {
        if (!activeNPCs.Contains(npc))
        {
            activeNPCs.Add(npc);
            UpdateIndicatorForNPC(npc); // Check immediately if they need an indicator
        }
    }

    private void UnregisterNPC(StepController npc)
    {
        if (activeNPCs.Contains(npc))
        {
            activeNPCs.Remove(npc);
            RemoveIndicatorForNPC(npc);
        }
    }

    private void UpdateAllIndicators()
    {
        foreach (var npc in activeNPCs)
        {
            UpdateIndicatorForNPC(npc);
        }
    }

    private void UpdateIndicatorForNPC(StepController npc)
    {
        if (questManager == null || npc == null || npc.Actor == null) 
        {
            Debug.LogWarning($"[QuestIndicatorManager] Cannot update indicator. questManager is null: {questManager == null}, npc is null: {npc == null}, npc.Actor is null: {npc?.Actor == null}");
            return;
        }

        string actorID = npc.Actor.ID;
        Debug.Log($"[QuestIndicatorManager] Checking indicator for NPC: {actorID}");
        
        QuestType? activeQuestType = questManager.GetActiveQuestTypeForActor(actorID);
        
        if (!activeQuestType.HasValue && dailyQuestManager != null && gameNarrativeData != null)
        {
            string trackedDaily = dailyQuestManager.SaveData.TrackedQuestID;
            if (!string.IsNullOrEmpty(trackedDaily) && gameNarrativeData.DailyQuestConfigs.TryGetValue(trackedDaily, out var config))
            {
                if (config.ObjectiveType == ObjectiveType.TalkToNPC && config.TargetID == actorID)
                {
                    int currentProgress = dailyQuestManager.SaveData.ActiveDailyQuests.GetValueOrDefault(trackedDaily, 0);
                    if (currentProgress < config.RequireAmount)
                    {
                        activeQuestType = QuestType.Daily;
                    }
                }
            }
        }

        // If the quest type changed or we need to update the UI
        RemoveIndicatorForNPC(npc);

        if (activeQuestType.HasValue)
        {
            Debug.Log($"[QuestIndicatorManager] NPC {actorID} needs an indicator of type: {activeQuestType.Value}");
            GameObject prefabToSpawn = null;
            if (activeQuestType.Value == QuestType.Main)
            {
                prefabToSpawn = mainQuestIndicatorPrefab;
            }
            else if (activeQuestType.Value == QuestType.Daily)
            {
                prefabToSpawn = dailyQuestIndicatorPrefab;
            }

            if (prefabToSpawn != null)
            {
                GameObject newIndicator = Instantiate(prefabToSpawn, npc.transform);
                newIndicator.transform.localPosition = indicatorOffset;
                activeIndicators[npc] = newIndicator;
                Debug.Log($"[QuestIndicatorManager] Spawend indicator for {actorID} successfully.");
            }
            else 
            {
                Debug.LogWarning($"[QuestIndicatorManager] Prefab to spawn is null for type {activeQuestType.Value}");
            }
        }
        else 
        {
            Debug.Log($"[QuestIndicatorManager] NPC {actorID} does not need any indicator.");
        }
    }

    private void RemoveIndicatorForNPC(StepController npc)
    {
        if (activeIndicators.ContainsKey(npc))
        {
            if (activeIndicators[npc] != null)
            {
                Destroy(activeIndicators[npc]);
            }
            activeIndicators.Remove(npc);
        }
    }
}
