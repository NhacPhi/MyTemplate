using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class QuestManager 
{
    private Dictionary<string, QuestLineConfig> questLines = new();
    [Inject] SaveSystem saveSystem;

    public QuestSaveData SaveData => saveSystem.Player.Quest;

    private QuestLineConfig currentQuestLine;
    private QuestComponent currentQuest;
    private StepComponent currentStep;
    
    private string currentQuestLineIndex;

    [Inject] GameNarrativeData gameNarrativeData;

    public void StartGame()
    {
        GameEvent.OnAcceptQuest += AcceptQuest;
        GameEvent.OnCompleteStep += EndStep;
        GameEvent.OnMakeWinChoice += MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent += CheckStepValidity;
        GameEvent.OnEndDialogue += EndDialogue;
        GameEvent.OnEnemyKilled += HandleEnemyKilled;
        GameEvent.OnPickupItem += HandlePickupItem;

        StartQuestLine();
    }

    ~QuestManager() {
        GameEvent.OnAcceptQuest -= AcceptQuest;
        GameEvent.OnCompleteStep -= EndStep;
        GameEvent.OnMakeWinChoice -= MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent -= CheckStepValidity;
        GameEvent.OnEndDialogue -= EndDialogue;
        GameEvent.OnEnemyKilled -= HandleEnemyKilled;
        GameEvent.OnPickupItem -= HandlePickupItem;
    }

    void StartQuestLine()
    {
        questLines = gameNarrativeData.QuestLineConfigs;

        if (questLines != null)
        {
            // Find first uncompleted QuestLine
            var firstUnfinished = questLines.FirstOrDefault(kvp => !SaveData.IsQuestLineCompleted(kvp.Key));

            if (firstUnfinished.Key != null)
            {
                currentQuestLine = firstUnfinished.Value;
                currentQuestLineIndex = firstUnfinished.Key;
                
                // Restore active quest if we have one in save data
                if (!string.IsNullOrEmpty(SaveData.ActiveQuestID))
                {
                    currentQuest = currentQuestLine.Quests.FirstOrDefault(q => q.ID == SaveData.ActiveQuestID);
                    if (currentQuest != null)
                    {
                        StartStep(SaveData.ActiveStepIndex);
                    }
                }
            }
            else
            {
                currentQuestLine = null;
                currentQuestLineIndex = null;
            }
        }
        GameEvent.OnQuestUpdated?.Invoke();
    }

    /// <summary>
    /// Gets the type of quest this actor is involved in (Main or Daily).
    /// Used by QuestIndicatorManager to display the ! or ? icon.
    /// </summary>
    public QuestType? GetActiveQuestTypeForActor(string actorID)
    {
        // 1. Check if they are involved in the CURRENT ACTIVE quest (The ? icon)
        if (currentQuest != null && currentStep != null)
        {
            if (currentStep.ActorID == actorID)
            {
                Debug.Log($"[QuestManager] GetActiveQuestTypeForActor({actorID}): NPC matches current active step!");
                return currentQuest.Type;
            }
            Debug.Log($"[QuestManager] GetActiveQuestTypeForActor({actorID}): Quest is active but step belongs to {currentStep.ActorID}, not this NPC.");
            // If they are not the actor for the active step, they don't get an icon.
            return null;
        }
        
        // 2. NGƯỜI CHƠI CHỈ MUỐN HIỂN THỊ KHI ĐANG LÀM NHIỆM VỤ
        // Bỏ qua việc check Quest mới để tắt hoàn toàn Prefab khi dừng theo dõi
        /*
        if (currentQuestLine != null)
        {
            var pendingQuest = currentQuestLine.Quests.FirstOrDefault(o => !SaveData.IsQuestCompleted(o.ID));
            if (pendingQuest != null && pendingQuest.Steps != null && pendingQuest.Steps.Count > 0)
            {
                if (pendingQuest.Steps[0].ActorID == actorID)
                {
                    Debug.Log($"[QuestManager] GetActiveQuestTypeForActor({actorID}): NPC matches next available quest giver!");
                    return pendingQuest.Type;
                }
            }
        }
        */
        
        Debug.Log($"[QuestManager] GetActiveQuestTypeForActor({actorID}): No active quest found for this NPC.");
        return null;
    }

    /// <summary>
    /// Checks whether a Quest meets all unlock prerequisites (Prerequisite Quest IDs and Required Level).
    /// </summary>
    public bool IsQuestUnlocked(QuestComponent quest, int playerLevel = 1)
    {
        if (quest == null) return false;

        // 1. Nếu đã hoàn thành quest này -> Không còn ở trạng thái "chờ nhận"
        if (SaveData.IsQuestCompleted(quest.ID)) return false;

        // 2. Kiểm tra Cấp độ nhân vật yêu cầu
        if (playerLevel < quest.RequiredLevel) return false;

        // 3. Kiểm tra các Quest tiên quyết (Prerequisites)
        if (quest.PrerequisiteQuestIDs != null && quest.PrerequisiteQuestIDs.Count > 0)
        {
            foreach (var reqQuestId in quest.PrerequisiteQuestIDs)
            {
                if (string.IsNullOrEmpty(reqQuestId)) continue;
                if (!SaveData.IsQuestCompleted(reqQuestId))
                {
                    return false; // Chưa hoàn thành Quest điều kiện
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a Quest that is currently available to be accepted from this actor.
    /// </summary>
    private QuestComponent GetAvailableQuestFromActor(string actorID, int playerLevel = 1)
    {
        if (currentQuestLine == null || currentQuest != null) return null;

        var pendingQuest = currentQuestLine.Quests.FirstOrDefault(o => 
            !SaveData.IsQuestCompleted(o.ID) && 
            IsQuestUnlocked(o, playerLevel)
        );

        if (pendingQuest != null && pendingQuest.Steps != null && pendingQuest.Steps.Count > 0)
        {
            if (pendingQuest.Steps[0].ActorID == actorID)
            {
                return pendingQuest;
            }
        }
        return null;
    }

    bool HasActiveStep(string actorToCheckWith)
    {
        if(currentQuest != null && currentStep != null)
        {
            if(currentStep.ActorID == actorToCheckWith)
            {
                return true;
            }
        }
        return false;
    }

    public DialogueConfig InteractWithCharacter(string actor, bool isCheckValidity, bool isValid)
    {
        // 1. If we don't have an active quest, check if the NPC has a quest to GIVE us.
        if(currentQuest == null)
        {
            QuestComponent availableQuest = GetAvailableQuestFromActor(actor);
            if (availableQuest != null)
            {
                // [TẠM THỜI BỎ QUA UI NHẬN QUEST]
                // Trigger the UI to ask the player to accept the quest.
                // GameEvent.OnOpenQuestUI?.Invoke(availableQuest);
                
                // Return null so StepController plays the default dialogue
                return null; 
            }
        }

        // 2. If we DO have an active quest, check if this NPC is part of the current step
        if(HasActiveStep(actor))
        {
            if(isCheckValidity)
            {
                if(isValid)
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue);
                else
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.IncompleteDialogue);
            }
            else
            {
                return gameNarrativeData.GetDialogueConfigByID(currentStep.PreviousDialogue);
            }
        }
        
        return null;
    }

    public void AcceptQuest(string questID)
    {
        Debug.Log($"[QuestManager] AcceptQuest() triggered for ID: {questID}");

        // Cho phép nhận hoặc chuyển qua theo dõi quest khác
        foreach (var kvp in questLines)
        {
            QuestComponent questToStart = kvp.Value.Quests.FirstOrDefault(q => q.ID == questID);
            if (questToStart != null)
            {
                Debug.Log($"[QuestManager] Found quest {questID} in QuestLine '{kvp.Key}'. Registering as Active Quest...");
                currentQuestLine = kvp.Value;
                currentQuestLineIndex = kvp.Key;
                currentQuest = questToStart;
                SaveData.ActiveQuestID = questToStart.ID;
                
                // Nếu chưa có tiến trình thì bắt đầu từ Step 0, nếu có thể sau này nâng cấp lưu Step riêng biệt
                StartStep(0);
                saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                Debug.Log($"[QuestManager] Quest {questID} has been successfully started at Step 0!");
                return;
            }
        }
        
        Debug.LogWarning($"[QuestManager] ERROR: Could not find quest with ID '{questID}' in any loaded QuestLines!");
    }

    public void StopTrackingQuest()
    {
        Debug.Log($"[QuestManager] StopTrackingQuest() triggered");
        currentQuest = null;
        SaveData.ActiveQuestID = string.Empty;
        currentStep = null;
        saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        GameEvent.OnQuestUpdated?.Invoke();
    }

    void MakeWinningChoice()
    {
        CheckStepValidity();
    }

    void CheckStepValidity()
    {
        if (currentStep != null)
        {
            switch (currentStep.Type)
            {
                case StepType.Dialogue:
                case StepType.TalkToNPC:
                    if (!string.IsNullOrEmpty(currentStep.CompletedDialogue) && gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue) != null)
                    {
                        // Waiting for player to complete the CompletedDialogue
                    }
                    else
                    {
                        EndStep();
                    }
                    break;

                case StepType.DefeatEnemy:
                case StepType.CollectItem:
                case StepType.CheckItem:
                case StepType.GiveItem:
                case StepType.ReachLocation:
                case StepType.InteractObject:
                default:
                    // These steps require objective progress (killing enemies, collecting items, etc.), NOT finished by dialogue start!
                    break;
            }
        }
    }

    private void HandleEnemyKilled(string enemyID, int amount)
    {
        if (currentStep == null) return;
        if (currentStep.Type == StepType.DefeatEnemy)
        {
            if (string.IsNullOrEmpty(currentStep.TargetID) || currentStep.TargetID == enemyID)
            {
                SaveData.ActiveStepProgress += amount;
                if (SaveData.ActiveStepProgress >= currentStep.RequiredAmount)
                {
                    EndStep();
                }
                else
                {
                    saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                    GameEvent.OnQuestUpdated?.Invoke();
                }
            }
        }
    }

    private void HandlePickupItem(string itemID, int amount)
    {
        if (currentStep == null) return;
        if (currentStep.Type == StepType.CollectItem || currentStep.Type == StepType.CheckItem)
        {
            string targetItem = !string.IsNullOrEmpty(currentStep.ItemID) ? currentStep.ItemID : currentStep.TargetID;
            if (string.IsNullOrEmpty(targetItem) || targetItem == itemID)
            {
                SaveData.ActiveStepProgress += amount;
                if (SaveData.ActiveStepProgress >= currentStep.RequiredAmount)
                {
                    EndStep();
                }
                else
                {
                    saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                    GameEvent.OnQuestUpdated?.Invoke();
                }
            }
        }
    }

    void StartStep(int index)
    {
        if (currentQuest != null && currentQuest.Steps != null && currentQuest.Steps.Count > index)
        {
            SaveData.ActiveStepIndex = index;
            SaveData.ActiveStepProgress = 0;
            currentStep = currentQuest.Steps[index];
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            GameEvent.OnQuestUpdated?.Invoke();
        }
    }

    void EndStep()
    {
        currentStep = null;
        if (currentQuest != null)
        {
            int nextStepIndex = SaveData.ActiveStepIndex + 1;
            if (currentQuest.Steps.Count > nextStepIndex)
            {
                StartStep(nextStepIndex);
            }
            else
            {
                EndQuest();
            }
        }
        GameEvent.OnQuestUpdated?.Invoke();
    }

    void EndQuest()
    {
        if (currentQuest != null)
        {
            if (!string.IsNullOrEmpty(currentQuest.RewardID))
            {
                Debug.Log($"[QuestManager] Quest completed! Granting reward: {currentQuest.RewardID}");
            }
            
            SaveData.CompleteQuest(currentQuest.ID);
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        }
        
        currentQuest = null;
        currentStep = null;

        if (currentQuestLine != null)
        {
            var pendingQuest = currentQuestLine.Quests.FirstOrDefault(o => !SaveData.IsQuestCompleted(o.ID));
            if (pendingQuest == null)
            {
                EndQuestline();
            }
        }
        
        GameEvent.OnQuestUpdated?.Invoke();
    }

    void EndQuestline()
    {
        if (currentQuestLine != null)
        {
            SaveData.CompleteQuestLine(currentQuestLineIndex);
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            
            StartQuestLine(); // Automatically start the next questline
        }
        GameEvent.OnQuestUpdated?.Invoke();
    }

    void EndDialogue(DialogueType dialogueType)
    {
        if (currentStep == null) return;

        // Steps are ONLY completed when explicitly reading a Completion dialogue or clicking CompleteStep choice button
        if (dialogueType == DialogueType.Completion)
        {
            EndStep();
        }
    }
}
