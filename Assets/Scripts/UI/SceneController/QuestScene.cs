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
    [SerializeField] private Transform questListContainer;
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
    [SerializeField] private ItemUI rewardItemPrefab;

    [Inject] private UIManager uiManager;
    
    private QuestManager questManager;
    private DailyQuestManager dailyQuestManager;
    private GameNarrativeData gameNarrativeData;
    private GameDataBase gameDataBase;
    private InventoryManager inventoryManager;
    private CurrencyManager currencyManager;

    private bool isDailyTab = false;
    private string currentlySelectedDailyQuestId;
    private List<DailyQuestItemUI> dailyItemUIs = new List<DailyQuestItemUI>();

    private void EnsureDependencies()
    {
        if (questManager == null && GameplayScope.Instance != null)
        {
            questManager = GameplayScope.Instance.Container.Resolve<QuestManager>();
            dailyQuestManager = GameplayScope.Instance.Container.Resolve<DailyQuestManager>();
            gameNarrativeData = GameplayScope.Instance.Container.Resolve<GameNarrativeData>();
            gameDataBase = GameplayScope.Instance.Container.Resolve<GameDataBase>();
            inventoryManager = GameplayScope.Instance.Container.Resolve<InventoryManager>();
            currencyManager = GameplayScope.Instance.Container.Resolve<CurrencyManager>();
        }
    }

    private QuestCompoment currentlySelectedQuest;

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
        // Clear old items
        if (questListContainer != null)
        {
            foreach (Transform child in questListContainer)
            {
                Destroy(child.gameObject);
            }
        }
        dailyItemUIs.Clear();

        rightPaneRoot.SetActive(false);

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
                    DailyQuestItemUI itemUI = Instantiate(dailyQuestItemPrefab, questListContainer);
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
        QuestCompoment firstVisibleQuest = null;

        // Render Active and Available Quests
        if (questManager != null && gameNarrativeData != null)
        {
            foreach (var kvp in gameNarrativeData.QuestLineConfigs)
            {
                QuestLineConfig questLine = kvp.Value;
                bool hasVisibleQuests = false;
                QuestLineUIGroup group = null;

                foreach (var quest in questLine.Quests)
                {
                    // Only show Active or Available quests (or completed if you want a log)
                    bool isActive = questManager.SaveData.ActiveQuestID == quest.ID;
                    bool isCompleted = questManager.SaveData.IsQuestCompleted(quest.ID);
                    
                    // Lọc logic hiển thị: Nếu chưa hoàn thành
                    if (!isCompleted)
                    {
                        if (firstVisibleQuest == null) firstVisibleQuest = quest;

                        if (!hasVisibleQuests)
                        {
                            // Tạo Header của QuestLine
                            group = Instantiate(questLineGroupPrefab, questListContainer);
                            string groupName = LocalizationManager.Instance.GetLocalizedValue(questLine.Name);
                            group.Setup(string.IsNullOrEmpty(groupName) ? questLine.ID : groupName);
                            hasVisibleQuests = true;
                        }

                        // Tạo nút Quest bên trong Group
                        QuestItemUI itemUI = Instantiate(questItemPrefab, group.GetContainer());
                        string questName = LocalizationManager.Instance.GetLocalizedValue(quest.Name);
                        itemUI.Setup(quest, string.IsNullOrEmpty(questName) ? quest.ID : questName, OnSelectQuest);
                        
                        // Kích hoạt icon nếu quest đang được theo dõi
                        itemUI.SetActiveState(isActive);
                        
                        // Tự động select Quest đang Active
                        if (isActive)
                        {
                            OnSelectQuest(quest);
                        }
                    }
                }
            }
        }

        // Nếu bảng bên phải chưa được bật (tức là không có Quest Active nào), thì tự động chọn Quest đầu tiên
        if (!rightPaneRoot.activeSelf && firstVisibleQuest != null)
        {
            OnSelectQuest(firstVisibleQuest);
        }
    }

    private void UpdateRightPane(string title, string description, string location, string objective, bool isTracked, bool canClaimReward, bool isCompleted, string rewardId)
    {
        rightPaneRoot.SetActive(true);

        if (txtQuestTitle != null) txtQuestTitle.text = string.IsNullOrEmpty(title) ? "Nhiệm vụ" : title;
        if (txtQuestDescription != null) txtQuestDescription.text = string.IsNullOrEmpty(description) ? "Không có mô tả" : description;
        if (txtQuestLocation != null) txtQuestLocation.text = string.IsNullOrEmpty(location) ? "Không rõ" : location;
        if (txtCurrentObjective != null) txtCurrentObjective.text = string.IsNullOrEmpty(objective) ? "Không có mục tiêu" : objective;

        if (btnAcceptOrTrack != null && txtBtnAcceptTrack != null)
        {
            btnAcceptOrTrack.gameObject.SetActive(!isCompleted);

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

    private void OnSelectQuest(QuestCompoment quest)
    {
        currentlySelectedQuest = quest;

        string questName = LocalizationManager.Instance.GetLocalizedValue(quest.Name);
        string questDesc = LocalizationManager.Instance.GetLocalizedValue(quest.Description);
        string location = "Không rõ";
        string objective = "Không có mục tiêu";

        if (quest.Steps != null && quest.Steps.Count > 0)
        {
            int stepIndex = (questManager.SaveData.ActiveQuestID == quest.ID) ? questManager.SaveData.ActiveStepIndex : 0;
            if (stepIndex < quest.Steps.Count)
            {
                StepCompoment step = quest.Steps[stepIndex];
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
                string followingStr = LocalizationManager.Instance.GetLocalizedValue("STR_FOLLOWING");

                if (step.Type == StepType.Dialogue)
                    objective = $"{talkToStr} {actorName}";
                else if (step.Type == StepType.GiveItem)
                    objective = $"{giveItemToStr} {actorName}";
                else
                    objective = $"{followingStr} {actorName}";
            }
        }

        bool isTracked = questManager.SaveData.ActiveQuestID == quest.ID;
        bool isCompleted = questManager.SaveData.IsQuestCompleted(quest.ID);
        UpdateRightPane(questName, questDesc, location, objective, isTracked, false, isCompleted, quest.RewardID);
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
                            ItemUI itemUI = Instantiate(rewardItemPrefab, rewardListContainer);
                            itemUI.Init(reward.ItemID, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG, reward.Amount);
                            // Tắt icon mảnh tướng nếu không cần
                            itemUI.ActiveFragIcon(itemConfig.Type == ItemType.Shard);
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
            Debug.Log($"[QuestScene] Clicked Accept/Track for Quest ID: {currentlySelectedQuest.ID}");
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
