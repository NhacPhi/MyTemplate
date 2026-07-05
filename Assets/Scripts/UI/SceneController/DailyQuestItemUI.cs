using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyQuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private Button btnSelect;
    [SerializeField] private GameObject highlightObj; // Object to show when selected (e.g. outline/background)
    [SerializeField] private GameObject activeIcon; // Object to show when tracked

    private string currentQuestId;
    private Action<string> onSelectCallback;

    public string QuestId => currentQuestId;

    public void Setup(string questId, string localizedName, Action<string> onSelect)
    {
        currentQuestId = questId;
        onSelectCallback = onSelect;

        if (txtQuestName != null)
        {
            txtQuestName.text = localizedName;
        }

        if (btnSelect != null)
        {
            btnSelect.onClick.RemoveAllListeners();
            btnSelect.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        onSelectCallback?.Invoke(currentQuestId);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightObj != null)
        {
            highlightObj.SetActive(isHighlighted);
        }
        
        if (txtQuestName != null)
        {
            txtQuestName.color = isHighlighted ? Color.black : Color.white;
        }
    }

    public void SetActiveState(bool isActive)
    {
        if (activeIcon != null)
        {
            activeIcon.SetActive(isActive);
        }
    }
}
