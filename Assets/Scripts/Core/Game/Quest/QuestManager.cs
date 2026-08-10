using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using deVoid.Utils;
using Cysharp.Threading.Tasks;

public class QuestManager 
{
    private Dictionary<string, QuestLineConfig> questLines = new();
    [Inject] SaveSystem saveSystem;

    public QuestSaveData SaveData => saveSystem.Player.Quest;

    private QuestLineConfig currentQuestLine;
    private QuestComponent currentQuest;
    private StepComponent currentStep;
    
    private string currentQuestLineIndex;

    public QuestComponent CurrentQuest => currentQuest;
    public bool IsMainQuestActive => currentQuest != null && (currentQuest.Type == QuestType.Main || currentQuest.Type == QuestType.None);

    [Inject] GameNarrativeData gameNarrativeData;
    [Inject] GameDataBase gameDataBase;

    private string pendingCompletedDialogue = null;
    private string dialogueOwnerStepID = null;

    public void StartGame()
    {
        GameEvent.OnAcceptQuest += AcceptQuest;
        GameEvent.OnRejectQuest += RejectQuest;
        GameEvent.OnCompleteStep += EndStep;
        GameEvent.OnMakeWinChoice += MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent += CheckStepValidity;
        GameEvent.OnEndDialogue += EndDialogue;
        GameEvent.OnSceneReady += OnSceneReady;
        Signals.Get<EnemyKilledSignal>().AddListener(HandleEnemyKilled);
        Signals.Get<PickupItemSignal>().AddListener(HandlePickupItem);
        Signals.Get<WinBattleSignal>().AddListener(OnWinBattle);

        StartQuestLine();
    }

    ~QuestManager() {
        GameEvent.OnAcceptQuest -= AcceptQuest;
        GameEvent.OnRejectQuest -= RejectQuest;
        GameEvent.OnCompleteStep -= EndStep;
        GameEvent.OnMakeWinChoice -= MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent -= CheckStepValidity;
        GameEvent.OnEndDialogue -= EndDialogue;
        GameEvent.OnSceneReady -= OnSceneReady;
        Signals.Get<EnemyKilledSignal>().RemoveListener(HandleEnemyKilled);
        Signals.Get<PickupItemSignal>().RemoveListener(HandlePickupItem);
        Signals.Get<WinBattleSignal>().RemoveListener(OnWinBattle);
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
    /// Checks if the specified actor is currently involved in an active quest step.
    /// </summary>
    public bool IsNPCInQuestProgress(string actorID)
    {
        if (string.IsNullOrEmpty(actorID)) return false;
        return GetActiveQuestTypeForActor(actorID).HasValue;
    }

    /// <summary>
    /// Checks whether a Quest is locked based on ChapterID, PrerequisiteQuestIDs, and RequiredLevel.
    /// Để MỞ KHÓA, Quest BẮT BUỘC PHẢI THỎA MÃN TẤT CẢ CÁC ĐIỀU KIỆN (AND logic).
    /// Nếu điều kiện nào bỏ trống/rỗng -> Tự động qua (Auto pass điều kiện đó).
    /// </summary>
    public bool IsQuestLocked(QuestComponent quest, int playerLevel = 1)
    {
        if (quest == null) return false;

        // 1. Điều kiện ChapterID (Bắt buộc QuestLine ChapterID này phải hoàn thành)
        if (!string.IsNullOrEmpty(quest.ChapterID))
        {
            if (!SaveData.IsQuestLineCompleted(quest.ChapterID))
            {
                return true; // Vi phạm -> Bị khóa ngay
            }
        }

        // 2. Điều kiện PrerequisiteQuestIDs (Bắt buộc TẤT CẢ các Quest tiên quyết phải hoàn thành)
        if (quest.PrerequisiteQuestIDs != null && quest.PrerequisiteQuestIDs.Count > 0)
        {
            foreach (var reqQuestId in quest.PrerequisiteQuestIDs)
            {
                if (string.IsNullOrEmpty(reqQuestId)) continue;
                if (!SaveData.IsQuestCompleted(reqQuestId))
                {
                    return true; // Chỉ cần 1 Quest chưa xong -> Bị khóa ngay
                }
            }
        }

        // 3. Điều kiện Cấp độ nhân vật yêu cầu
        if (playerLevel < quest.RequiredLevel)
        {
            return true; // Chưa đủ Cấp độ -> Bị khóa ngay
        }

        // Chỉ khi THỎA MÃN TẤT CẢ các điều kiện khai báo -> Mới Mở Khóa (Not locked)
        return false;
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

        // 3. Kiểm tra khóa theo ChapterID và PrerequisiteQuestIDs
        if (IsQuestLocked(quest)) return false;

        return true;
    }

    /// <summary>
    /// Returns a Quest that is currently available to be accepted from this actor.
    /// </summary>
    private QuestComponent GetAvailableQuestFromActor(string actorID, int playerLevel = 1)
    {
        if (currentQuest != null) return null;
        if (questLines == null || questLines.Count == 0) return null;

        string cleanActorId = actorID?.Trim();

        foreach (var questLineKvp in questLines)
        {
            var qLine = questLineKvp.Value;
            if (qLine == null || qLine.Quests == null) continue;

            var pendingQuest = qLine.Quests.FirstOrDefault(o => 
                !SaveData.IsQuestCompleted(o.ID) && 
                IsQuestUnlocked(o, playerLevel)
            );

            if (pendingQuest != null && pendingQuest.Steps != null && pendingQuest.Steps.Count > 0)
            {
                if (string.Equals(pendingQuest.Steps[0].ActorID?.Trim(), cleanActorId, StringComparison.OrdinalIgnoreCase))
                {
                    return pendingQuest;
                }
            }
        }
        return null;
    }

    bool HasActiveStep(string actorToCheckWith)
    {
        if (currentQuest != null && currentStep != null)
        {
            if (string.Equals(currentStep.ActorID?.Trim(), actorToCheckWith?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public DialogueConfig InteractWithCharacter(string actor, bool isCheckValidity, bool isValid)
    {
        // 1. If we don't have an active quest, check if the NPC has a quest to GIVE us.
        if (currentQuest == null)
        {
            QuestComponent availableQuest = GetAvailableQuestFromActor(actor);
            if (availableQuest != null)
            {
                Debug.Log($"[QuestManager] Player interacting with NPC '{actor}'. Accepting available quest '{availableQuest.ID}'");
                AcceptQuest(availableQuest.ID);
            }
        }

        // 2. If we DO have an active quest, check if this NPC is part of the current step
        if (HasActiveStep(actor))
        {
            dialogueOwnerStepID = currentStep?.ID;

            if (isCheckValidity)
            {
                if (isValid)
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue);
                else
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.IncompleteDialogue);
            }
            else
            {
                DialogueConfig dialogue = gameNarrativeData.GetDialogueConfigByID(currentStep.PreviousDialogue);
                if (dialogue == null && !string.IsNullOrEmpty(currentStep.IncompleteDialogue))
                {
                    dialogue = gameNarrativeData.GetDialogueConfigByID(currentStep.IncompleteDialogue);
                }
                if (dialogue == null && !string.IsNullOrEmpty(currentStep.CompletedDialogue))
                {
                    dialogue = gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue);
                }

                if (dialogue != null)
                {
                    return dialogue;
                }
                else
                {
                    Debug.LogWarning($"[QuestManager] Active step found for NPC '{actor}' in Quest '{currentQuest?.ID}' (Step ID: '{currentStep?.ID}'), but no valid dialogue config was found (PreviousDialogue: '{currentStep?.PreviousDialogue}', Incomplete: '{currentStep?.IncompleteDialogue}', Completed: '{currentStep?.CompletedDialogue}').");
                }
            }
        }
        else if (currentQuest != null)
        {
            Debug.Log($"[QuestManager] Player interacted with NPC '{actor}', but current active step (Step ID: '{currentStep?.ID}') belongs to ActorID '{currentStep?.ActorID}', not '{actor}'.");
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

    private bool isStepRejected = false;

    public void RejectQuest(string questID)
    {
        Debug.Log($"[QuestManager] RejectQuest() triggered for ID: '{questID}'");
        isStepRejected = true;

        if (!string.IsNullOrEmpty(questID) && currentQuest != null && string.Equals(currentQuest.ID, questID, StringComparison.OrdinalIgnoreCase))
        {
            StopTrackingQuest();
        }
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
        if (currentStep == null || currentQuest == null) return;

        StepComponent completedStep = currentStep;
        currentStep = null;
        dialogueOwnerStepID = null;

        int nextStepIndex = SaveData.ActiveStepIndex + 1;
        Debug.Log($"[QuestManager] EndStep() completed step '{completedStep.ID}'. Next step index: {nextStepIndex} (Total steps: {currentQuest.Steps.Count})");

        if (currentQuest.Steps.Count > nextStepIndex)
        {
            StartStep(nextStepIndex);
        }
        else
        {
            EndQuest();
        }
        GameEvent.OnQuestUpdated?.Invoke();
    }

    private void GrantQuestReward(string rewardID)
    {
        if (string.IsNullOrEmpty(rewardID)) return;

        Debug.Log($"[QuestManager] GrantQuestReward triggered for RewardID: '{rewardID}'");
        var rewardConfig = (gameDataBase != null) ? gameDataBase.GetRewardConfig(rewardID) : null;
        if (rewardConfig != null && rewardConfig.Rewards != null)
        {
            List<RewardItemData> rewards = new List<RewardItemData>();
            foreach (var r in rewardConfig.Rewards)
            {
                if (r == null || string.IsNullOrEmpty(r.ItemID) || r.Amount <= 0) continue;

                // Grant reward item via GameEvent.OnRequestPickupItem (handled by InventoryManager)
                GameEvent.OnRequestPickupItem?.Invoke(r.ItemID, r.Amount);
                rewards.Add(new RewardItemData(r.ItemID, r.Amount));
                Debug.Log($"[QuestManager] Granted reward item: {r.ItemID} x{r.Amount}");
            }

            // Show Receive Item Popup if UIManager is available
            if (rewards.Count > 0)
            {
                UIManager uiManager = null;
                if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
                {
                    try { uiManager = GameplayScope.Instance.Container.Resolve<UIManager>(); } catch { }
                }
                if (uiManager != null)
                {
                    uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(rewards));
                }
            }
        }
        else
        {
            Debug.LogWarning($"[QuestManager] RewardConfig for ID '{rewardID}' was not found in GameNarrativeData!");
        }
    }


    public bool ClaimQuestReward(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return false;

        QuestComponent quest = null;
        if (questLines != null)
        {
            foreach (var kvp in questLines)
            {
                if (kvp.Value?.Quests == null) continue;
                quest = kvp.Value.Quests.FirstOrDefault(q => q.ID == questID);
                if (quest != null) break;
            }
        }

        string rewardID = quest != null ? quest.RewardID : null;
        if (!string.IsNullOrEmpty(rewardID))
        {
            GrantQuestReward(rewardID);
        }

        SaveData.CompleteQuest(questID);
        saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        GameEvent.OnQuestUpdated?.Invoke();
        Debug.Log($"[QuestManager] ClaimQuestReward successfully claimed reward and showed popup for Quest '{questID}'!");
        return true;
    }

    void EndQuest()
    {
        if (currentQuest != null)
        {
            Debug.Log($"[QuestManager] EndQuest() called for Quest '{currentQuest.ID}'. Quest steps completed! Marking as Claimable.");
            
            if (!SaveData.ClaimableQuestIDs.Contains(currentQuest.ID) && !SaveData.IsQuestCompleted(currentQuest.ID))
            {
                SaveData.ClaimableQuestIDs.Add(currentQuest.ID);
            }
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        }
        
        currentQuest = null;
        currentStep = null;

        if (currentQuestLine != null)
        {
            var pendingQuest = currentQuestLine.Quests.FirstOrDefault(o => !SaveData.IsQuestCompleted(o.ID) && !SaveData.IsQuestClaimable(o.ID));
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
            if (!string.IsNullOrEmpty(currentQuestLineIndex))
            {
                SaveData.CompleteQuestLine(currentQuestLineIndex);
            }
            if (!string.IsNullOrEmpty(currentQuestLine.ID))
            {
                SaveData.CompleteQuestLine(currentQuestLine.ID);
            }
            saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
            
            StartQuestLine(); // Automatically start the next questline
        }
        GameEvent.OnQuestUpdated?.Invoke();
    }

    private void OnWinBattle(string battleID, int amount)
    {
        if (currentStep == null) return;
        if (currentStep.Type == StepType.DefeatEnemy)
        {
            if (string.IsNullOrEmpty(currentStep.TargetID) || string.Equals(currentStep.TargetID.Trim(), battleID.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                SaveData.ActiveStepProgress += amount;
                if (SaveData.ActiveStepProgress >= currentStep.RequiredAmount)
                {
                    Debug.Log($"[QuestManager] WinBattleSignal received for battle '{battleID}'. Progress requirement met!");
                    if (!string.IsNullOrEmpty(currentStep.CompletedDialogue))
                    {
                        pendingCompletedDialogue = currentStep.CompletedDialogue;
                        Debug.Log($"[QuestManager] Pending CompletedDialogue '{pendingCompletedDialogue}' set to play after returning to location scene.");
                        saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                    }
                    else
                    {
                        EndStep();
                    }
                }
                else
                {
                    saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
                    GameEvent.OnQuestUpdated?.Invoke();
                }
            }
        }
    }

    private async void OnSceneReady()
    {
        if (!string.IsNullOrEmpty(pendingCompletedDialogue))
        {
            // Yield 1 frame so Protagonist and all scene entities execute OnEnable/Start and register event listeners first
            await UniTask.Yield();

            string dialogueToPlay = pendingCompletedDialogue;
            pendingCompletedDialogue = null;

            DialogueConfig config = gameNarrativeData.GetDialogueConfigByID(dialogueToPlay);
            if (config != null)
            {
                dialogueOwnerStepID = currentStep?.ID;
                Debug.Log($"[QuestManager] Playing pending CompletedDialogue '{dialogueToPlay}' after returning from battle scene.");
                UIManager uiManager = null;
                if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
                {
                    try { uiManager = GameplayScope.Instance.Container.Resolve<UIManager>(); } catch { }
                }
                if (uiManager != null)
                {
                    uiManager.OpenWindowScene(ScreenIds.DialogueScene);
                }
                GameEvent.OnStartDialogue?.Invoke(config);
            }
            else
            {
                Debug.LogWarning($"[QuestManager] Pending CompletedDialogue '{dialogueToPlay}' was not found in DialogueConfigs! Completing step directly.");
                EndStep();
            }
        }
    }

    public async void TriggerBattleFromQuestStep(string battleID)
    {
        if (string.IsNullOrEmpty(battleID)) return;

        // Defer 1 frame so DialogueScene can finish closing first
        await UniTask.Yield();

        BattleSessionContext sessionContext = null;
        UIManager uiManager = null;
        SceneLoader sceneLoader = null;

        if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            try { sessionContext = GameplayScope.Instance.Container.Resolve<BattleSessionContext>(); } catch { }
            try { uiManager = GameplayScope.Instance.Container.Resolve<UIManager>(); } catch { }
            try { sceneLoader = GameplayScope.Instance.Container.Resolve<SceneLoader>(); } catch { }
        }

        if (sessionContext != null)
        {
            sessionContext.PendingBattleID = battleID;

            GameSceneSO prevLoc = null;
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                prevLoc = SceneLoader.GetRegisteredScene(activeScene.name);
            }
            if (prevLoc == null && sceneLoader != null)
            {
                prevLoc = sceneLoader.CurrentLoadedScene;
            }
            if (prevLoc == null && SceneLoader.LastLoadedLocation != null)
            {
                prevLoc = SceneLoader.LastLoadedLocation;
            }

            sessionContext.PreviousLocation = prevLoc;
            sessionContext.PreviousLocationName = prevLoc != null ? prevLoc.name : null;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                sessionContext.ReturnPosition = playerObj.transform.position;
                if (Camera.main != null)
                {
                    sessionContext.ReturnCameraPosition = Camera.main.transform.position;
                }
            }

            Debug.Log($"[QuestManager] Triggering PrepareBattleScene for DefeatEnemy Quest Step (BattleID: '{battleID}')");

            if (uiManager != null)
            {
                uiManager.OpenWindowScene(ScreenIds.PrepareBattleScene);
            }

            UIEvent.OnPrepareBattleData?.Invoke();
        }
        else
        {
            Debug.LogError($"[QuestManager] Could not resolve BattleSessionContext to trigger battle '{battleID}'!");
        }
    }

    void EndDialogue(DialogueType dialogueType)
    {
        if (currentStep == null) return;

        string lastDialogueOwner = dialogueOwnerStepID;
        dialogueOwnerStepID = null;

        if (isStepRejected)
        {
            isStepRejected = false;
            Debug.Log($"[QuestManager] EndDialogue skipped because player rejected the quest/step.");
            return;
        }

        // Verify that the dialogue that just ended belonged to currentStep before any step completion occurred
        bool isSameStepDialogue = !string.IsNullOrEmpty(lastDialogueOwner) && string.Equals(lastDialogueOwner, currentStep.ID, StringComparison.OrdinalIgnoreCase);
        if (!isSameStepDialogue)
        {
            Debug.Log($"[QuestManager] EndDialogue skipped because active step changed or already completed during dialogue (Opened for: '{lastDialogueOwner}', Current Step: '{currentStep.ID}')");
            return;
        }

        // 1. Completion dialogue completes the current step
        if (dialogueType == DialogueType.Completion)
        {
            EndStep();
            return;
        }

        // 2. Dialogue or TalkToNPC steps COMPLETE when their dialogue finishes reading
        if (currentStep.Type == StepType.Dialogue || currentStep.Type == StepType.TalkToNPC)
        {
            EndStep();
            return;
        }

        // 3. DefeatEnemy steps trigger PrepareBattleScene after DialogueBeforeStep finishes (if battle requirement not met yet)
        if (currentStep.Type == StepType.DefeatEnemy && !string.IsNullOrEmpty(currentStep.TargetID))
        {
            if (SaveData.ActiveStepProgress >= currentStep.RequiredAmount)
            {
                EndStep();
            }
            else
            {
                TriggerBattleFromQuestStep(currentStep.TargetID.Trim());
            }
            return;
        }
    }
}
