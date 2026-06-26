using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VContainer;
public class InteractionOption : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI content = default;
    [SerializeField] private Image icon;
    [SerializeField] private InteractionType type = default;

    [Inject] UIManager uiManager;
    
    public Interaction TargetInteraction { get; private set; }
    public InteractionType DefaultType => type;
    public InteractionType Type { get { return TargetInteraction != null ? TargetInteraction.type : type; }}
    
    public void Setup(Interaction interaction)
    {
        TargetInteraction = interaction;
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetContent(string id)
    {
        content.text = LocalizationManager.Instance.GetLocalizedValue(id);
    }

    public void SetContentText(string text)
    {
        content.text = text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click Interaction Option");
        InteractionType currentType = Type;
        switch(currentType)
        {
            case InteractionType.Talk:
                uiManager.OpenWindowScene(ScreenIds.DialogueScene);
                if (TargetInteraction != null)
                    GameEvent.OnExecuteSpecificInteraction?.Invoke(TargetInteraction);
                else
                    GameEvent.OnInteraction?.Invoke();
                break;
            case InteractionType.PickUp:
                if (TargetInteraction != null)
                    GameEvent.OnExecuteSpecificInteraction?.Invoke(TargetInteraction);
                else
                    GameEvent.OnInteraction?.Invoke();
                break;

            case InteractionType.Cook:
                if (TargetInteraction != null)
                    GameEvent.OnExecuteSpecificInteraction?.Invoke(TargetInteraction);
                else
                    GameEvent.OnInteraction?.Invoke();
                break;

            case InteractionType.Fighting:
                if (TargetInteraction != null)
                    GameEvent.OnExecuteSpecificInteraction?.Invoke(TargetInteraction);
                else
                    GameEvent.OnInteraction?.Invoke();
                break;
        }
    }
}
