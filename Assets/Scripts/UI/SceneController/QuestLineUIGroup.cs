using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestLineUIGroup : MonoBehaviour
{
    [SerializeField] private Button btnToggle;
    [SerializeField] private TextMeshProUGUI txtQuestLineName;
    [SerializeField] private Transform questItemsContainer;
    
    private bool isOpen = true;

    private void Awake()
    {
        if (btnToggle != null) btnToggle.onClick.AddListener(ToggleGroup);
    }

    public void Setup(string localizedGroupName)
    {
        if (txtQuestLineName != null) txtQuestLineName.text = localizedGroupName;
        if (questItemsContainer != null) questItemsContainer.gameObject.SetActive(true);
        isOpen = true;
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
