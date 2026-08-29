using TMPro;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnGamePanel;

    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnCatchSkill;
    [SerializeField] private Button btnMap;
    [SerializeField] private Button btnQuest;
    [SerializeField] private Button btnSevenDayLogin;

    [Header("Quest Tracker")]
    [SerializeField] private TextMeshProUGUI txtCurrentMainQuest;
    [SerializeField] private GameObject mainQuestTrackerRoot;
    [SerializeField] private Button btnTrackedQuestClick;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private SaveSystem saveSystem;

    private bool hasCheckedAutoSevenDayLogin = false;

    protected override void Awake()
    {
        base.Awake();
        UIEvent.OnToggleGamePlayScene += ToggleScene;
        GameEvent.OnQuestUpdated += UpdateMainQuestTracker;
        GameEvent.OnCompleteStep += OnStepCompleted;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UIEvent.OnToggleGamePlayScene -= ToggleScene;
        GameEvent.OnQuestUpdated -= UpdateMainQuestTracker;
        GameEvent.OnCompleteStep -= OnStepCompleted;
    }

    private void OnStepCompleted()
    {
        UpdateMainQuestTracker();
    }

    private void ToggleScene(bool show)
    {
        gameObject.SetActive(show);
        if (show)
        {
            UpdateMainQuestTracker();
        }
    }

    private void Start()
    {
        if (btnGamePanel != null)
        {
            btnGamePanel.onClick.AddListener(() =>
            {
                uiManager.CloseAllWindows();
                uiManager.ShowPanel(ScreenIds.GamePlayPanel);
            });
        }

        if (btnAttack != null)
        {
            btnAttack.onClick.AddListener(() => { GameEvent.OnPlayerAttack?.Invoke(); });
        }

        if (btnCatchSkill != null)
        {
            btnCatchSkill.onClick.AddListener(() => { GameEvent.OnPlayerTransform?.Invoke(); });
        }
        
        if (btnMap != null)
        {
            btnMap.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.MapScene);
            });
        }
        
        if (btnQuest != null)
        {
            btnQuest.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.QuestScene);
            });
        }

        if (btnSevenDayLogin != null)
        {
            btnSevenDayLogin.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.SevenDayLogin);
            });
        }

        // Tự động tìm txtCurrentMainQuest nếu chưa gán Inspector
        if (txtCurrentMainQuest == null)
        {
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t.name.Contains("CurrentMainQuest") || t.name.Contains("MainQuest") || t.name.Contains("TrackedQuest"))
                {
                    txtCurrentMainQuest = t;
                    break;
                }
            }
        }

        if (btnTrackedQuestClick == null && txtCurrentMainQuest != null)
        {
            btnTrackedQuestClick = txtCurrentMainQuest.GetComponent<Button>();
            if (btnTrackedQuestClick == null && txtCurrentMainQuest.transform.parent != null)
            {
                btnTrackedQuestClick = txtCurrentMainQuest.transform.parent.GetComponent<Button>();
            }
        }

        if (btnTrackedQuestClick != null)
        {
            btnTrackedQuestClick.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.QuestScene);
            });
        }

        UpdateMainQuestTracker();
        CheckAutoSevenDayLogin();
    }

    private async void CheckAutoSevenDayLogin()
    {
        if (hasCheckedAutoSevenDayLogin) return;
        hasCheckedAutoSevenDayLogin = true;

        // Chờ 0.3s để GamePlayScene hoàn thành transition mượt mà và gameplay scene đã ổn định
        await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(0.3f));
        if (this == null || !gameObject.activeInHierarchy) return;

        var save = saveSystem ?? SaveSystem.Instance;
        if (SevenDayLoginScene.HasUnclaimedRewards(save))
        {
            if (uiManager != null)
            {
                uiManager.OpenWindowScene(ScreenIds.SevenDayLogin);
            }
        }
    }

    private void OnEnable()
    {
        UpdateMainQuestTracker();
    }

    public void UpdateMainQuestTracker()
    {
        QuestManager questManager = null;
        GameNarrativeData gameNarrativeData = null;

        if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            try { questManager = GameplayScope.Instance.Container.Resolve<QuestManager>(); } catch { }
            try { gameNarrativeData = GameplayScope.Instance.Container.Resolve<GameNarrativeData>(); } catch { }
        }

        if (questManager == null || questManager.SaveData == null || string.IsNullOrEmpty(questManager.SaveData.ActiveQuestID))
        {
            SetTrackerActive(false);
            return;
        }

        string activeQuestId = questManager.SaveData.ActiveQuestID;

        // Nếu quest đã hoàn thành và nhận thưởng xong -> deactive tracker
        if (questManager.SaveData.IsQuestCompleted(activeQuestId))
        {
            SetTrackerActive(false);
            return;
        }

        QuestComponent quest = questManager.CurrentQuest;
        if (quest == null)
        {
            quest = questManager.GetQuestByID(activeQuestId);
        }

        if (quest == null)
        {
            SetTrackerActive(false);
            return;
        }

        bool canClaim = questManager.SaveData.IsQuestClaimable(activeQuestId);

        string questName = LocalizationManager.Instance != null 
            ? LocalizationManager.Instance.GetLocalizedValue(quest.Name) 
            : quest.Name.ToString();
        if (string.IsNullOrEmpty(questName)) questName = quest.ID;

        string statusText = "";
        if (canClaim)
        {
            string claimStr = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetLocalizedValue("STR_CLAIM_REWARD") 
                : "Nhận Thưởng";
            if (string.IsNullOrEmpty(claimStr) || claimStr == "STR_CLAIM_REWARD") claimStr = "Nhận Thưởng";
            statusText = $"<color=#FFD700>[{claimStr}]</color>";
        }
        else
        {
            StepComponent step = questManager.CurrentStep;
            if (step == null && quest.Steps != null && quest.Steps.Count > 0)
            {
                int stepIndex = questManager.SaveData.ActiveStepIndex;
                if (stepIndex >= 0 && stepIndex < quest.Steps.Count)
                {
                    step = quest.Steps[stepIndex];
                }
            }

            if (step != null)
            {
                statusText = FormatStepObjective(step, gameNarrativeData);
            }
        }

        SetTrackerActive(true);
        if (txtCurrentMainQuest != null)
        {
            txtCurrentMainQuest.text = $"<b>{questName}</b>\n{statusText}";
        }
    }

    private string FormatStepObjective(StepComponent step, GameNarrativeData narrativeData)
    {
        if (step == null) return string.Empty;

        string actorName = step.ActorID;
        if (narrativeData != null && !string.IsNullOrEmpty(step.ActorID))
        {
            var actor = narrativeData.GetActorConfig(step.ActorID);
            if (actor != null && LocalizationManager.Instance != null)
            {
                string locName = LocalizationManager.Instance.GetLocalizedValue(actor.Name);
                if (!string.IsNullOrEmpty(locName)) actorName = locName;
            }
        }

        string talkToStr = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetLocalizedValue("STR_TALK_TO") : "Nói chuyện với";
        string defeatEnemyStr = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetLocalizedValue("STR_DEFEAT_ENEMY") : "Tiêu diệt";
        string giveItemToStr = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetLocalizedValue("STR_GIVE_ITEM_TO") : "Giao vật phẩm cho";

        if (string.IsNullOrEmpty(talkToStr) || talkToStr == "STR_TALK_TO") talkToStr = "Nói chuyện với";
        if (string.IsNullOrEmpty(defeatEnemyStr) || defeatEnemyStr == "STR_DEFEAT_ENEMY") defeatEnemyStr = "Tiêu diệt";
        if (string.IsNullOrEmpty(giveItemToStr) || giveItemToStr == "STR_GIVE_ITEM_TO") giveItemToStr = "Giao vật phẩm cho";

        switch (step.Type)
        {
            case StepType.Dialogue:
            case StepType.TalkToNPC:
                return $"{talkToStr} {actorName}";
            case StepType.GiveItem:
                return $"{giveItemToStr} {actorName}";
            case StepType.DefeatEnemy:
                return $"{defeatEnemyStr} {actorName}";
            case StepType.CollectItem:
            case StepType.CheckItem:
                string itemTarget = !string.IsNullOrEmpty(step.ItemID) ? step.ItemID : (!string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "vật phẩm");
                return $"Thu thập {itemTarget} ({step.RequiredAmount})";
            case StepType.ReachLocation:
                string locTarget = !string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "khu vực chỉ định";
                return $"Di chuyển đến {locTarget}";
            case StepType.InteractObject:
                string objTarget = !string.IsNullOrEmpty(step.TargetID) ? step.TargetID : "vật thể";
                return $"Kích hoạt {objTarget}";
            default:
                string desc = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetLocalizedValue(step.Description) : step.Description.ToString();
                return !string.IsNullOrEmpty(desc) ? desc : $"{talkToStr} {actorName}";
        }
    }

    private void SetTrackerActive(bool active)
    {
        if (mainQuestTrackerRoot != null)
        {
            mainQuestTrackerRoot.SetActive(active);
        }
        else if (txtCurrentMainQuest != null)
        {
            txtCurrentMainQuest.gameObject.SetActive(active);
        }
    }
}
