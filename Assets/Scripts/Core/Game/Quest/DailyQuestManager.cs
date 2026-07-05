using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class DailyQuestManager
{
    [Inject] private SaveSystem saveSystem;
    [Inject] private GameNarrativeData narrativeData;

    public DailyQuestSaveData SaveData => saveSystem.Player.DailyQuest;

    public void StartGame()
    {
        CheckAndResetDaily();
        RegisterEvents();
    }

    private void CheckAndResetDaily()
    {
        DateTime now = DateTime.Now;
        // Check if we passed 4 AM today
        DateTime resetTimeToday = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);
        
        // If it's before 4 AM right now, the last reset time was yesterday 4 AM
        if (now < resetTimeToday)
        {
            resetTimeToday = resetTimeToday.AddDays(-1);
        }

        long currentResetTicks = resetTimeToday.Ticks;

        if (SaveData.LastResetTimeTicks < currentResetTicks)
        {
            // We passed a new 4 AM reset boundary!
            Debug.Log("[DailyQuestManager] Resetting Daily Quests!");
            SaveData.LastResetTimeTicks = currentResetTicks;
            SaveData.ActiveDailyQuests.Clear();
            SaveData.CompletedDailyQuests.Clear();
            SaveData.TrackedQuestID = string.Empty;

            GenerateRandomDailyQuests();
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        }
        else if (SaveData.ActiveDailyQuests.Count == 0 && SaveData.CompletedDailyQuests.Count == 0)
        {
            // Fallback: If no quests are active or completed for today, generate them.
            Debug.Log("[DailyQuestManager] No quests found for today. Generating random daily quests.");
            GenerateRandomDailyQuests();
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        }
    }

    private void GenerateRandomDailyQuests()
    {
        if (narrativeData.DailyQuestConfigs == null || narrativeData.DailyQuestConfigs.Count == 0) return;

        // Shuffle and pick 3
        var allKeys = narrativeData.DailyQuestConfigs.Keys.ToList();
        var random = new System.Random();
        var shuffled = allKeys.OrderBy(x => random.Next()).ToList();

        int count = Mathf.Min(3, shuffled.Count);
        for (int i = 0; i < count; i++)
        {
            SaveData.ActiveDailyQuests[shuffled[i]] = 0; // Progress starts at 0
        }
    }

    private void RegisterEvents()
    {
        GameEvent.OnEnemyKilled += (enemyId, amount) => UpdateProgress(ObjectiveType.KillEnemy, enemyId, amount);
        GameEvent.OnPickupItem += (itemId, amount) => UpdateProgress(ObjectiveType.PickupItem, itemId, amount);
        GameEvent.OnCharacterUpgraded += (charId, amount) => UpdateProgress(ObjectiveType.UpgradeCharacter, charId, amount);
        GameEvent.OnWeaponUpgraded += (amount) => UpdateProgress(ObjectiveType.UpgradeWeapon, string.Empty, amount);
        GameEvent.OnWinBattle += (stageId, amount) => UpdateProgress(ObjectiveType.WinBattle, stageId, amount);
        GameEvent.OnShopPurchased += (itemId, amount) => UpdateProgress(ObjectiveType.Purchase, itemId, amount);
        GameEvent.OnGachaSummoned += (type, amount) => UpdateProgress(ObjectiveType.Summon, type, amount);
        
        // Custom hook for talking to NPC
        GameEvent.OnOpenDialogue += (dialogueId, actorConfig) => 
        {
            if (actorConfig != null)
                UpdateProgress(ObjectiveType.TalkToNPC, actorConfig.ActorSo.ID, 1);
        };
    }

    private void UpdateProgress(ObjectiveType type, string targetId, int amount)
    {
        bool updated = false;
        var keys = SaveData.ActiveDailyQuests.Keys.ToList();

        foreach (var questId in keys)
        {
            if (narrativeData.DailyQuestConfigs.TryGetValue(questId, out var config))
            {
                if (config.ObjectiveType == type)
                {
                    // Check if TargetID matches or is empty (meaning any target counts)
                    if (string.IsNullOrEmpty(config.TargetID) || config.TargetID == targetId)
                    {
                        int currentProgress = SaveData.ActiveDailyQuests[questId];
                        if (currentProgress < config.RequireAmount)
                        {
                            SaveData.ActiveDailyQuests[questId] = Mathf.Min(currentProgress + amount, config.RequireAmount);
                            updated = true;
                            Debug.Log($"[DailyQuestManager] Updated progress for {questId}: {SaveData.ActiveDailyQuests[questId]}/{config.RequireAmount}");
                        }
                    }
                }
            }
        }

        if (updated)
        {
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            GameEvent.OnDailyQuestUpdated?.Invoke();
        }
    }

    public void TrackQuest(string questId)
    {
        SaveData.TrackedQuestID = questId;
        saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        GameEvent.OnDailyQuestUpdated?.Invoke();
        GameEvent.OnQuestUpdated?.Invoke(); // Also trigger main quest update to refresh indicators
    }

    public void StopTracking()
    {
        SaveData.TrackedQuestID = string.Empty;
        saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        GameEvent.OnDailyQuestUpdated?.Invoke();
        GameEvent.OnQuestUpdated?.Invoke(); // Also trigger main quest update to refresh indicators
    }

    public bool ClaimReward(string questId)
    {
        if (SaveData.ActiveDailyQuests.TryGetValue(questId, out int progress))
        {
            if (narrativeData.DailyQuestConfigs.TryGetValue(questId, out var config))
            {
                if (progress >= config.RequireAmount)
                {
                    // Claim reward
                    SaveData.ActiveDailyQuests.Remove(questId);
                    SaveData.CompletedDailyQuests.Add(questId);
                    
                    if (SaveData.TrackedQuestID == questId)
                    {
                        StopTracking();
                    }

                    if (!string.IsNullOrEmpty(config.RewardID))
                    {
                        Debug.Log($"[DailyQuestManager] Claimed reward {config.RewardID} for quest {questId}");
                        // TODO: Grant reward via InventoryManager
                    }

                    saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                    GameEvent.OnDailyQuestUpdated?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }
}
