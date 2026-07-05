using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private Button btnSelect;
    [SerializeField] private GameObject activeIcon;

    private QuestCompoment currentQuest;
    private Action<QuestCompoment> onSelectCallback;

    public void Setup(QuestCompoment quest, string localizedName, Action<QuestCompoment> onSelect)
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
