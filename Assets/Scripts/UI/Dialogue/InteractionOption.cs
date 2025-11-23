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
    public InteractionType Type { get { return type; }}
    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetContent(string id)
    {
        content.text = LocalizationManager.Instance.GetLocalizedValue(id);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click Interaction Option");
        switch(type)
        {
            case InteractionType.Talk:
                uiManager.OpenWindowScene(ScreenIds.DialogueScene);
                GameEvent.OnInteraction?.Invoke();
                break;
            case InteractionType.PickUp:
                
                break;
            case InteractionType.Cook:

                break;
        }
    }
}
