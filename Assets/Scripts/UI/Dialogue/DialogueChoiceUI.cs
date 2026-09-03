using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueChoiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Button button;

    public void FillChoice(ChoiceComponent choice)
    {
        content.text = LocalizationManager.Instance != null 
            ? LocalizationManager.Instance.GetLocalizedValue(choice.Text)
            : choice.Text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            GameEvent.OnMakeChoiceUI?.Invoke(choice);
        });
    }

    public void RemoveEventOfButton()
    {
        button.onClick.RemoveAllListeners();
    }
}
