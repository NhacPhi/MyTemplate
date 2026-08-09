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

    [Inject] UIManager uiManager;

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

    // Event show Choices
    // Start is called before the first frame update
    private void Awake()
    {
        GameEvent.OnOpenDialogue += OpenUIDialogue;
        GameEvent.OnEndDialogue += CloseUIDialogue;

        GameEvent.OnShowChoiceUI += ShowChoices;
    }

    private void OnDestroy()
    {
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
                if(typeWriteEffect.IsCompleted)
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

    private void OpenUIDialogue(string str, ActorConfig actor)
    {
        SetDialogue(str, actor);
    }

    void CloseUIDialogue(DialogueType type)
    {
        uiManager.CloseWindowScene(ScreenIds.DialogueScene);
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
    }

    public void SetDialogue(string str, ActorConfig actor)
    {
        UpdateSkipButtonState();
        choicesManager.DisableAllCHoiceUI();
        choicesManager.gameObject.SetActive(false);
        typeWriteEffect.Play(str);
        avatarActor.sprite = actor.ActorSo.Texture;
        nameActor.text = LocalizationManager.Instance.GetLocalizedValue(actor.Name);
    }

    private void ShowChoices(List<ChoiceComponent> choices)
    {
        choicesManager.FillChoices(choices);
        choicesManager.gameObject.SetActive(true);
    }
}
