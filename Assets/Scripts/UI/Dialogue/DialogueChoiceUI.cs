using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueChoiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Button button;

    public void FillChoice(ChoiceData choice)
    {
        content.text = LocalizationManager.Instance.GetLocalizedValue(choice.Reponse);
        button.onClick.AddListener(() =>
        {
            GameEvent.OnMakeChocieUI(choice);
        });
    }

    public void RemoveEventOfButton()
    {
        button.onClick.RemoveAllListeners();
    }
}
