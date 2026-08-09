using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private Button btnSelect;
    [SerializeField] private GameObject highlightObj;
    [SerializeField] private GameObject activeIcon;
    [SerializeField] private TextMeshProUGUI txtLocked;
    [SerializeField] private GameObject lockObj;

    private QuestComponent currentQuest;
    private Action<QuestComponent> onSelectCallback;

    public QuestComponent Quest => currentQuest;

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

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightObj != null)
        {
            highlightObj.SetActive(isHighlighted);
        }
        
        if (txtQuestName != null)
        {
            txtQuestName.color = isHighlighted ? Color.white : Color.black;
        }
    }

    public void SetActiveState(bool isActive)
    {
        if (activeIcon != null)
        {
            activeIcon.SetActive(isActive);
        }
    }

    public void SetLockState(bool isLocked, string lockMessage = "")
    {
        if (lockObj != null)
        {
            lockObj.SetActive(isLocked);
        }

        if (txtLocked != null)
        {
            txtLocked.gameObject.SetActive(isLocked);
            if (isLocked && !string.IsNullOrEmpty(lockMessage))
            {
                txtLocked.text = lockMessage;
            }
        }
    }
}
