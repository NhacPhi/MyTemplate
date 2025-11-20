using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueChoiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Button button;

    public void SetContent(string str, UnityAction action)
    {
        content.text = str;
        button.onClick.AddListener(action);
    }
}
