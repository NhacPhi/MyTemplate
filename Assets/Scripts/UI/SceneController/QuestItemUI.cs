using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private Button btnSelect;
    [SerializeField] private GameObject activeIcon;

    private QuestComponent currentQuest;
    private Action<QuestComponent> onSelectCallback;

    public void Setup(QuestComponent quest, string localizedName, Action<QuestComponent> onSelect)
    {
        currentQuest = quest;
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
        onSelectCallback?.Invoke(currentQuest);
    }

    public void SetActiveState(bool isActive)
    {
        if (activeIcon != null)
        {
            activeIcon.SetActive(isActive);
        }
    }
}
