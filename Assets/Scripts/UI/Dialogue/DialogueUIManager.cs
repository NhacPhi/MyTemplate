using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DialogueUIManager : MonoBehaviour
{
    [SerializeField] private TypewriterEffect typeWriteEffect;
    [SerializeField] private Image avatarActor;
    [SerializeField] private TextMeshProUGUI nameActor;
    [SerializeField] private Button btnAdvance;
    [SerializeField] private Button btnSkip;

    [SerializeField] private DialogueChoicesUIManager choicesManager;

    [Inject] private UIManager uiManager;

    private void UpdateSkipButtonState()
    {
        if (btnSkip == null) return;

        QuestManager qMgr = null;
        if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            try
            {
                qMgr = GameplayScope.Instance.Container.Resolve<QuestManager>();
            }
            catch { }
        }

        bool isMainQuest = qMgr != null && qMgr.IsMainQuestActive;
        btnSkip.gameObject.SetActive(!isMainQuest);
        btnSkip.interactable = !isMainQuest;
    }

    private void Awake()
    {
        GameEvent.OnStartDialogue += HandleStartDialogue;
        GameEvent.OnOpenDialogue += OpenUIDialogue;
        GameEvent.OnEndDialogue += CloseUIDialogue;
        GameEvent.OnShowChoiceUI += ShowChoices;
    }

    private void OnDestroy()
    {
        GameEvent.OnStartDialogue -= HandleStartDialogue;
        GameEvent.OnOpenDialogue -= OpenUIDialogue;
        GameEvent.OnEndDialogue -= CloseUIDialogue;
        GameEvent.OnShowChoiceUI -= ShowChoices;
    }

    void Start()
    {
        if (btnAdvance != null)
        {
            btnAdvance.onClick.AddListener(() =>
            {
                if (typeWriteEffect.IsCompleted)
                {
                    GameEvent.OnAdvanceDialogueEvent?.Invoke();
                }
                else
                {
                    typeWriteEffect.Skip();
                }
            });
        }

        if (btnSkip != null)
        {
            btnSkip.onClick.AddListener(SkipDialogue);
        }
    }

    private void SkipDialogue()
    {
        GameEvent.OnEndDialogue?.Invoke(DialogueType.Default);
    }

    private void HandleStartDialogue(DialogueConfig config)
    {
        EnsureDialogueWindowOpen();
    }

    private void OpenUIDialogue(string str, ActorConfig actor)
    {
        EnsureDialogueWindowOpen();
        SetDialogue(str, actor);
    }

    private void EnsureDialogueWindowOpen()
    {
        if (uiManager == null && GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            try { uiManager = GameplayScope.Instance.Container.Resolve<UIManager>(); } catch { }
        }

        if (uiManager != null)
        {
            uiManager.OpenWindowScene(ScreenIds.DialogueScene);
        }
    }

    void CloseUIDialogue(DialogueType type)
    {
        if (uiManager == null && GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            try { uiManager = GameplayScope.Instance.Container.Resolve<UIManager>(); } catch { }
        }

        if (uiManager != null)
        {
            uiManager.CloseWindowScene(ScreenIds.DialogueScene);
        }
    }

    public void SetDialogue(string str, ActorConfig actor)
    {
        UpdateSkipButtonState();
        if (choicesManager != null)
        {
            choicesManager.DisableAllCHoiceUI();
            choicesManager.gameObject.SetActive(false);
        }
        if (typeWriteEffect != null)
        {
            typeWriteEffect.Play(str);
        }
        if (avatarActor != null && actor != null && actor.ActorSo != null)
        {
            avatarActor.sprite = actor.ActorSo.Texture;
        }
        if (nameActor != null && actor != null)
        {
            nameActor.text = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetLocalizedValue(actor.Name) 
                : actor.Name.ToString();
        }
    }

    private void ShowChoices(List<ChoiceComponent> choices)
    {
        if (choicesManager != null)
        {
            choicesManager.FillChoices(choices);
            choicesManager.gameObject.SetActive(true);
        }
    }
}
