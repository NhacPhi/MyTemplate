using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestLineUIGroup : MonoBehaviour
{
    [SerializeField] private Button btnToggle;
    [SerializeField] private TextMeshProUGUI txtQuestLineName;
    [SerializeField] private TextMeshProUGUI txtQuestLineDes;
    [SerializeField] private Transform questItemsContainer;
    
    private bool isOpen = true;

    private void Awake()
    {
        if (btnToggle != null) btnToggle.onClick.AddListener(ToggleGroup);
    }

    public void Setup(string localizedGroupName, string localizedGroupDes = "", bool startOpen = true)
    {
        if (txtQuestLineName != null) txtQuestLineName.text = localizedGroupName;
        if (txtQuestLineDes != null)
        {
            txtQuestLineDes.text = localizedGroupDes;
            txtQuestLineDes.gameObject.SetActive(!string.IsNullOrEmpty(localizedGroupDes));
        }
        SetOpenState(startOpen);
    }

    public void SetOpenState(bool open)
    {
        isOpen = open;
        if (questItemsContainer != null)
        {
            questItemsContainer.gameObject.SetActive(open);
        }

        if (transform.parent != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }

    public void ToggleGroup()
    {
        if (questItemsContainer == null) return;
        
        isOpen = !isOpen;
        questItemsContainer.gameObject.SetActive(isOpen);

        // Ép Layout tổng tính toán lại dồn chỗ ngay lập tức
        if (transform.parent != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }

    public Transform GetContainer()
    {
        return questItemsContainer != null ? questItemsContainer : transform;
    }
}
