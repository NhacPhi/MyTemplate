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

    [SerializeField] private DialogueChoicesUIManager choicesManager;

    [Inject] UIManager uiManager;
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

    private void OpenUIDialogue(string str, ActorConfig actor)
    {
        SetDialogue(str, actor);
    }

    void CloseUIDialogue(DialogueType type)
    {
        uiManager.CloseWindowScene(ScreenIds.DialogueScene);
    }

    public void SetDialogue(string str, ActorConfig actor)
    {
        choicesManager.DisableAllCHoiceUI();
        choicesManager.gameObject.SetActive(false);
        typeWriteEffect.Play(str);
        avatarActor.sprite = actor.ActorSo.Texture;
        nameActor.text = LocalizationManager.Instance.GetLocalizedValue(actor.Name);
    }

    private void ShowChoices(List<ChoiceCompement> choices)
    {
        choicesManager.FillChoices(choices);
        choicesManager.gameObject.SetActive(true);
    }
}
