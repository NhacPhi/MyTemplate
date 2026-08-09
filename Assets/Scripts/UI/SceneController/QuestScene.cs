using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

public class QuestScene : WindowController
{
    [Header("UI General")]
    [SerializeField] private Button btnClose;

    [Header("Tabs")]
    [SerializeField] private QuestToggle toggleTabMain;
    [SerializeField] private QuestToggle toggleTabDaily;

    [Header("Left Pane - List")]
    [SerializeField] private Transform questListContainer; // Daily Quest list container (Cái cũ)
    [SerializeField] private Transform mainQuestListContainer; // Main Quest list container (Cái mới)
    [SerializeField] private GameObject dailyQuestPanel; // Panel chứa Daily Quest (deactive khi chuyển tab)
    [SerializeField] private GameObject mainQuestPanel; // Panel chứa Main Quest (deactive khi chuyển tab)
    [SerializeField] private QuestLineUIGroup questLineGroupPrefab;
    [SerializeField] private QuestItemUI questItemPrefab;
    [SerializeField] private DailyQuestItemUI dailyQuestItemPrefab;

    [Header("Right Pane - Details")]
    [SerializeField] private GameObject rightPaneRoot;
    [SerializeField] private TextMeshProUGUI txtQuestTitle;
    [SerializeField] private TextMeshProUGUI txtQuestLocation;
    [SerializeField] private TextMeshProUGUI txtCurrentObjective;
    [SerializeField] private TextMeshProUGUI txtQuestDescription;
    
    [Header("Right Pane - Actions")]
    [SerializeField] private Button btnAcceptOrTrack;
    [SerializeField] private TextMeshProUGUI txtBtnAcceptTrack;

    [Header("Right Pane - Rewards")]
    [SerializeField] private Transform rewardListContainer;
    [SerializeField] private GameItemUI rewardItemPrefab;

    [Inject] private UIManager uiManager;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private CurrencyManager currencyManager;

    private QuestManager questManager;
    private DailyQuestManager dailyQuestManager;
    private GameNarrativeData gameNarrativeData;

    private bool isDailyTab = false;
    private string currentlySelectedDailyQuestId;
    private List<DailyQuestItemUI> dailyItemUIs = new List<DailyQuestItemUI>();
    private List<QuestItemUI> mainItemUIs = new List<QuestItemUI>();

    private void EnsureDependencies()
    {
        if (questManager == null && GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            questManager = GameplayScope.Instance.Container.Resolve<QuestManager>();
            dailyQuestManager = GameplayScope.Instance.Container.Resolve<DailyQuestManager>();
            gameNarrativeData = GameplayScope.Instance.Container.Resolve<GameNarrativeData>();
        }
    }

    private QuestComponent currentlySelectedQuest;

    private void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClose);
        }

        if (btnAcceptOrTrack != null)
        {
            btnAcceptOrTrack.onClick.AddListener(OnClickAcceptOrTrack);
        }

        if (toggleTabMain != null) 
        {
            toggleTabMain.Toggle.onValueChanged.AddListener((isOn) => { if (isOn) SwitchTab(false); });
        }
        if (toggleTabDaily != null) 
        {
            toggleTabDaily.Toggle.onValueChanged.AddListener((isOn) => { if (isOn) SwitchTab(true); });
        }
    }

    private void SwitchTab(bool isDaily)
    {
        isDailyTab = isDaily;
        currentlySelectedQuest = null;
        currentlySelectedDailyQuestId = string.Empty;
        RefreshQuestList();
    }

    private void OnClose()
    {
        uiManager.CloseWindowScene(ScreenIds.QuestScene);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        EnsureDependencies();
        RefreshQuestList();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void RefreshQuestList()
    {
        // Clear old items in Daily container (cái cũ)
        if (questListContainer != null)
        {
            foreach (Transform child in questListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Clear old items in Main container (cái mới)
        if (mainQuestListContainer != null)
        {
            foreach (Transform child in mainQuestListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        dailyItemUIs.Clear();
        mainItemUIs.Clear();
        rightPaneRoot.SetActive(false);

        // Đổi trạng thái Bật / Tắt (Active / Deactive) của 2 Object Panel khi chuyển Tab
        if (dailyQuestPanel != null) dailyQuestPanel.SetActive(isDailyTab);
        if (mainQuestPanel != null) mainQuestPanel.SetActive(!isDailyTab);

        if (isDailyTab)
        {
            RenderDailyQuests();
        }
        else
        {
            RenderMainQuests();
        }
    }

    private void RenderDailyQuests()
    {
        if (dailyQuestManager == null || gameNarrativeData == null) return;

        Transform container = questListContainer;
        if (container == null) return;

        string firstDailyQuestId = null;
        var activeQuests = dailyQuestManager.SaveData.ActiveDailyQuests;

        List<string> allQuestsToDisplay = new List<string>();
        allQuestsToDisplay.AddRange(activeQuests.Keys);

        if (allQuestsToDisplay.Count > 0)
        {
            foreach (var qId in allQuestsToDisplay)
            {
                if (firstDailyQuestId == null) firstDailyQuestId = qId;

                if (gameNarrativeData.DailyQuestConfigs.TryGetValue(qId, out var config))
                {
                    DailyQuestItemUI itemUI = Instantiate(dailyQuestItemPrefab, container);
                    string qName = LocalizationManager.Instance.GetLocalizedValue(config.Name);
                    
                    bool isCompleted = dailyQuestManager.SaveData.CompletedDailyQuests.Contains(qId);
                    string displayName = string.IsNullOrEmpty(qName) ? qId : qName;
                    if (isCompleted)
                    {
                        displayName += " <color=green>(Hoàn Thành)</color>";
                    }

                    itemUI.Setup(qId, displayName, OnSelectDailyQuest);
                    
                    bool isTracked = dailyQuestManager.SaveData.TrackedQuestID == qId;
                    itemUI.SetActiveState(isTracked);
                    dailyItemUIs.Add(itemUI);

                    if (isTracked)
                    {
                        OnSelectDailyQuest(qId);
                    }
                }
            }
        }

        if (!rightPaneRoot.activeSelf && !string.IsNullOrEmpty(firstDailyQuestId))
        {
            OnSelectDailyQuest(firstDailyQuestId);
        }
    }

    private void RenderMainQuests()
    {
        Transform container = mainQuestListContainer != null ? mainQuestListContainer : questListContainer;
        if (container == null) return;

        QuestComponent firstVisibleQuest = null;
        QuestLineUIGroup firstGroup = null;
        QuestLineUIGroup activeGroup = null;

        // Render Each Quest with its own QuestLineGroup
        if (questManager != null && gameNarrativeData != null)
        {
            List<QuestLineUIGroup> allGroups = new List<QuestLineUIGroup>();

            foreach (var kvp in gameNarrativeData.QuestLineConfigs)
            {
                QuestLineConfig questLine = kvp.Value;
                if (questLine.Quests == null || questLine.Quests.Count == 0) continue;

                foreach (var quest in questLine.Quests)
                {
                    bool isActive = questManager.SaveData.ActiveQuestID == quest.ID;
                    bool isCompleted = questManager.SaveData.IsQuestCompleted(quest.ID);

                    if (firstVisibleQuest == null) firstVisibleQuest = quest;

                    // Mỗi Quest ứng với một QuestLineGroup
                    QuestLineUIGroup group = Instantiate(questLineGroupPrefab, container);
                    allGroups.Add(group);
                    if (firstGroup == null) firstGroup = group;
                    if (isActive) activeGroup = group;

                    string groupName = LocalizationManager.Instance.GetLocalizedValue(questLine.Name);
                    string groupDes = LocalizationManager.Instance.GetLocalizedValue(quest.Description);
                    group.Setup(
                        string.IsNullOrEmpty(groupName) ? questLine.ID : groupName,
                        string.IsNullOrEmpty(groupDes) ? "" : groupDes
                    );

                    // Tạo nút Quest bên trong Group của Quest này
                    QuestItemUI itemUI = Instantiate(questItemPrefab, group.GetContainer());
                    string questName = LocalizationManager.Instance.GetLocalizedValue(quest.Name);
                    string displayName = string.IsNullOrEmpty(questName) ? quest.ID : questName;
                    if (isCompleted)
                    {
                        displayName += " <color=green>(Hoàn Thành)</color>";
                    }

                    itemUI.Setup(quest, displayName, OnSelectQuest);
                    
                    // Kiểm tra trạng thái khóa (Lock) theo ChapterID và PrerequisiteQuestIDs
                    bool isLocked = questManager != null && questManager.IsQuestLocked(quest);
                    itemUI.SetLockState(isLocked);

                    // Kích hoạt icon nếu quest đang được theo dõi
                    itemUI.SetActiveState(isActive);
                    mainItemUIs.Add(itemUI);
                    
                    // Tự động select Quest đang Active
                    if (isActive)
                    {
                        OnSelectQuest(quest);
                    }
                }
            }

            // Quản lý trạng thái mở/đóng container của các Group:
            // Đóng tất cả trừ Quest đang được làm (activeGroup). Nếu không có Quest đang làm -> Mở Group đầu tiên (firstGroup).
            QuestLineUIGroup groupToExpand = (activeGroup != null) ? activeGroup : firstGroup;
            foreach (var g in allGroups)
            {
                g.SetOpenState(g == groupToExpand);
            }
        }

        // Nếu bảng bên phải chưa được bật (tức là không có Quest Active nào), thì tự động chọn Quest đầu tiên
        if (!rightPaneRoot.activeSelf && firstVisibleQuest != null)
        {
            OnSelectQuest(firstVisibleQuest);
        }
    }

    private void UpdateRightPane(string title, string description, string location, string objective, bool isTracked, bool canClaimReward, bool isCompleted, string rewardId, bool isLocked = false)
    {
        rightPaneRoot.SetActive(true);

        if (txtQuestTitle != null) txtQuestTitle.text = string.IsNullOrEmpty(title) ? "Nhiệm vụ" : title;
        if (txtQuestDescription != null) txtQuestDescription.text = string.IsNullOrEmpty(description) ? "Không có mô tả" : description;
        if (txtQuestLocation != null) txtQuestLocation.text = string.IsNullOrEmpty(location) ? "Không rõ" : location;
        if (txtCurrentObjective != null) txtCurrentObjective.text = string.IsNullOrEmpty(objective) ? "Không có mục tiêu" : objective;

        if (btnAcceptOrTrack != null && txtBtnAcceptTrack != null)
        {
            // Hiển thị nút nếu Quest chưa hoàn thành HOẶC có thể Nhận Thưởng
            btnAcceptOrTrack.gameObject.SetActive((!isCompleted || canClaimReward) && !isLocked);

            if (canClaimReward)
            {
                txtBtnAcceptTrack.text = LocalizationManager.Instance.GetLocalizedValue("STR_CLAIM_REWARD");
                if (string.IsNullOrEmpty(txtBtnAcceptTrack.text)) txtBtnAcceptTrack.text = "Nhận Thưởng";
                btnAcceptOrTrack.interactable = true;
            }
            else
            {
                string key = isTracked ? "STR_UNFOLLOW_NPC" : "STR_FOLLOW_NPC";
                txtBtnAcceptTrack.text = LocalizationManager.Instance.GetLocalizedValue(key);
                if (string.IsNullOrEmpty(txtBtnAcceptTrack.text)) txtBtnAcceptTrack.text = isTracked ? "Dừng theo dõi" : "Theo dõi";
                btnAcceptOrTrack.interactable = true;
            }
        }

        RenderRewards(rewardId);
    }

    private void OnSelectDailyQuest(string questId)
    {
        currentlySelectedDailyQuestId = questId;

        // Update Toggle Highlights
        foreach (var ui in dailyItemUIs)
        {
            ui.SetHighlight(ui.QuestId == questId);
        }

        if (gameNarrativeData.DailyQuestConfigs.TryGetValue(questId, out var config))
        {
            string qName = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            string qDesc = LocalizationManager.Instance.GetLocalizedValue(config.Description);
            string location = LocalizationManager.Instance.GetLocalizedValue(config.LocationHash);
            string targetStr = LocalizationManager.Instance.GetLocalizedValue(config.TargetHash);
            
            try 
            {
                if (!string.IsNullOrEmpty(targetStr) && !string.IsNullOrEmpty(qName) && qName.Contains("{0}")) 
                {
                    qName = string.Format(qName, targetStr);
                }
            } catch {}

            bool isCompleted = dailyQuestManager.SaveData.CompletedDailyQuests.Contains(questId);
            int currentProgress = isCompleted ? config.RequireAmount : dailyQuestManager.SaveData.ActiveDailyQuests.GetValueOrDefault(questId, 0);
            string progressColor = currentProgress >= config.RequireAmount ? "<color=green>" : "<color=white>";
            
            string objectiveBase = string.IsNullOrEmpty(targetStr) ? qName : targetStr;
            string objective = $"{objectiveBase} ({progressColor}{currentProgress}</color>/{config.RequireAmount})";

            bool canClaim = currentProgress >= config.RequireAmount && !isCompleted;
            bool isTracked = dailyQuestManager.SaveData.TrackedQuestID == questId;

            UpdateRightPane(qName, qDesc, location, objective, isTracked, canClaim, isCompleted, config.RewardID);
        }
    }

    private void OnSelectQuest(QuestComponent quest)
    {
        currentlySelectedQuest = quest;

        // Update Toggle Highlights
        foreach (var ui in mainItemUIs)
        {
            ui.SetHighlight(ui.Quest != null && ui.Quest.ID == quest.ID);
        }

        string questName = LocalizationManager.Instance.GetLocalizedValue(quest.Name);
        string stepDesc = "";
        string location = "Không rõ";
        string objective = "Không có mục tiêu";

        if (quest.Steps != null && quest.Steps.Count > 0)
        {
            int stepIndex = (questManager.SaveData.ActiveQuestID == quest.ID) ? questManager.SaveData.ActiveStepIndex : 0;
            if (stepIndex < quest.Steps.Count)
            {
                StepComponent step = quest.Steps[stepIndex];
                stepDesc = LocalizationManager.Instance.GetLocalizedValue(step.Description);

                ActorConfig actor = gameNarrativeData.GetActorConfig(step.ActorID);
                string actorName = step.ActorID;
                if (actor != null)
                {
                    string locName = LocalizationManager.Instance.GetLocalizedValue(actor.Name);
                    if (!string.IsNullOrEmpty(locName)) actorName = locName;

                    if (actor.LocationName != 0)
                    {
                        location = LocalizationManager.Instance.GetLocalizedValue(actor.LocationName);
                        if (string.IsNullOrEmpty(location)) location = actor.LocationName.ToString();
                    }
                }
                
                string talkToStr = LocalizationManager.Instance.GetLocalizedValue("STR_TALK_TO");
                string giveItemToStr = LocalizationManager.Instance.GetLocalizedValue("STR_GIVE_ITEM_TO");
                string defeatEnemyStr = LocalizationManager.Instance.GetLocalizedValue("STR_DEFEAT_ENEMY");
                string followingStr = LocalizationManager.Instance.GetLocalizedValue("STR_FOLLOWING");

                if (string.IsNullOrEmpty(defeatEnemyStr) || defeatEnemyStr == "STR_DEFEAT_ENEMY") defeatEnemyStr = "Tiêu diệt";

                switch (step.Type)
                {
                    case StepType.Dialogue:
                    case StepType.TalkToNPC:
                        objective = $"{talkToStr} {actorName}";
                        break;
                    case StepType.GiveItem:
                        objective = $"{giveItemToStr} {actorName}";
                        break;
                    case StepType.DefeatEnemy:
                        objective = $"{defeatEnemyStr} {step.ActorID}";
                        break;
                    case StepType.CollectItem:
                    case StepType.CheckItem:
                        string targetItemName = !string.IsNullOrEmpty(step.ItemID) ? step.ItemID : (!string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "vật phẩm");
                        objective = $"Thu thập {targetItemName} ({step.RequiredAmount})";
                        break;
                    case StepType.ReachLocation:
                        string locationTarget = !string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "khu vực chỉ định";
                        objective = $"Di chuyển đến {locationTarget}";
                        break;
                    case StepType.InteractObject:
                        string objectTarget = !string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "vật thể";
                        objective = $"Kích hoạt {objectTarget}";
                        break;
                    default:
                        objective = $"{followingStr} {actorName}";
                        break;
                }
            }
        }

        bool isTracked = questManager.SaveData.ActiveQuestID == quest.ID;
        bool isCompleted = questManager.SaveData.IsQuestCompleted(quest.ID);
        bool canClaim = questManager != null && questManager.SaveData.IsQuestClaimable(quest.ID);
        bool isLocked = questManager != null && questManager.IsQuestLocked(quest);
        UpdateRightPane(questName, stepDesc, location, objective, isTracked, canClaim, isCompleted, quest.RewardID, isLocked);
    }

    private void RenderRewards(string rewardId)
    {
        if (rewardListContainer != null)
        {
            foreach (Transform child in rewardListContainer)
            {
                Destroy(child.gameObject);
            }

            if (!string.IsNullOrEmpty(rewardId) && rewardItemPrefab != null && gameDataBase != null)
            {
                var rewardConfig = gameDataBase.GetRewardConfig(rewardId);
                if (rewardConfig != null && rewardConfig.Rewards != null)
                {
                    foreach (var reward in rewardConfig.Rewards)
                    {
                        var itemConfig = gameDataBase.GetItemConfig(reward.ItemID);
                        if (itemConfig != null)
                        {
                            GameItemUI itemUI = Instantiate(rewardItemPrefab, rewardListContainer);
                            itemUI.Setup(reward.ItemID, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                            itemUI.SetAmount(reward.Amount);
                            if (itemUI is ItemUI customItemUI)
                            {
                                customItemUI.ActiveFragIcon(itemConfig.Type == ItemType.Shard);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnClickAcceptOrTrack()
    {
        if (isDailyTab)
        {
            if (!string.IsNullOrEmpty(currentlySelectedDailyQuestId))
            {
                if (gameNarrativeData.DailyQuestConfigs.TryGetValue(currentlySelectedDailyQuestId, out var config))
                {
                    int currentProgress = dailyQuestManager.SaveData.ActiveDailyQuests.GetValueOrDefault(currentlySelectedDailyQuestId, 0);
                    if (currentProgress >= config.RequireAmount)
                    {
                        if (dailyQuestManager.ClaimReward(currentlySelectedDailyQuestId))
                        {
                            if (!string.IsNullOrEmpty(config.RewardID))
                            {
                                var rewardConfig = gameDataBase.GetRewardConfig(config.RewardID);
                                if (rewardConfig != null && rewardConfig.Rewards != null)
                                {
                                    List<RewardItemData> rewards = new List<RewardItemData>();
                                    foreach (var r in rewardConfig.Rewards)
                                    {
                                        rewards.Add(new RewardItemData(r.ItemID, r.Amount));
                                        var itemConfig = gameDataBase.GetItemConfig(r.ItemID);
                                        if (itemConfig != null && inventoryManager != null && currencyManager != null)
                                        {
                                            if (itemConfig.Type == ItemType.Weapon)
                                            {
                                                for (int i = 0; i < r.Amount; i++)
                                                {
                                                    inventoryManager.AddWeapon(new WeaponSaveData
                                                    {
                                                        UUID = System.Guid.NewGuid().ToString(),
                                                        TemplateID = r.ItemID,
                                                        CurrentLevel = 1
                                                    });
                                                }
                                            }
                                            else if (itemConfig.Type == ItemType.Armor)
                                            {
                                                for (int i = 0; i < r.Amount; i++)
                                                {
                                                    inventoryManager.AddArmor(new ArmorSaveData
                                                    {
                                                        UUID = System.Guid.NewGuid().ToString(),
                                                        TemplateID = r.ItemID,
                                                        Level = 1
                                                    });
                                                }
                                            }
                                            else if (itemConfig.Type == ItemType.Currency && System.Enum.TryParse<CurrencyType>(r.ItemID, true, out var rCurrency))
                                            {
                                                currencyManager.Add(rCurrency, r.Amount);
                                            }
                                            else
                                            {
                                                inventoryManager.AddStackableItem(r.ItemID, itemConfig.Type, r.Amount);
                                            }
                                        }
                                    }
                                    if (rewards.Count > 0 && uiManager != null)
                                    {
                                        uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(rewards));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isTracked = dailyQuestManager.SaveData.TrackedQuestID == currentlySelectedDailyQuestId;
                        if (isTracked) dailyQuestManager.StopTracking();
                        else dailyQuestManager.TrackQuest(currentlySelectedDailyQuestId);
                    }
                    RefreshQuestList();
                }
            }
            return;
        }
        if (currentlySelectedQuest != null)
        {
            Debug.Log($"[QuestScene] Clicked Accept/Track/Claim for Quest ID: {currentlySelectedQuest.ID}");

            if (questManager != null && questManager.SaveData.IsQuestClaimable(currentlySelectedQuest.ID))
            {
                Debug.Log($"[QuestScene] Claiming reward for Main Quest ID: {currentlySelectedQuest.ID}");
                questManager.ClaimQuestReward(currentlySelectedQuest.ID);
                RefreshQuestList();
                OnSelectQuest(currentlySelectedQuest);
                return;
            }

            bool isActive = questManager.SaveData.ActiveQuestID == currentlySelectedQuest.ID;
            if (!isActive)
            {
                Debug.Log($"[QuestScene] Quest {currentlySelectedQuest.ID} is not active yet. Calling questManager.AcceptQuest()...");
                // Accept the quest
                questManager.AcceptQuest(currentlySelectedQuest.ID);
                RefreshQuestList(); // Update Left pane
                OnSelectQuest(currentlySelectedQuest); // Refresh right pane
                Debug.Log($"[QuestScene] Refresh complete.");
            }
            else
            {
                Debug.Log($"[QuestScene] Quest {currentlySelectedQuest.ID} is ALREADY active. Stopping tracking...");
                questManager.StopTrackingQuest();
                RefreshQuestList(); // Update Left pane
                OnSelectQuest(currentlySelectedQuest); // Refresh right pane
                Debug.Log($"[QuestScene] Stop tracking complete.");
            }
        }
    }
}
