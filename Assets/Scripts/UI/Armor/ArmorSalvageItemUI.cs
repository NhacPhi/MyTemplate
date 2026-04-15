using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Item UI trong danh sách salvage.
/// Hiển thị icon, rarity, level, checkbox chọn/bỏ chọn, và giá trị ArmorPrimorite.
/// </summary>
public class ArmorSalvageItemUI : GameItemUI, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private GameObject checkMark;

    private string armorUUID;
    private bool isSelected;

    public string ArmorUUID => armorUUID;
    public bool IsSelected => isSelected;

    /// <summary>
    /// Callback khi item được chọn/bỏ chọn. Trả về (UUID, isSelected)
    /// </summary>
    public System.Action<string, bool> OnSelectionChanged;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level)
    {
        base.Setup(id, rare, icon, background);
        armorUUID = id;
        txtLevel.text = "+" + level.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelection();
    }

    /// <summary>
    /// Toggle chọn/bỏ chọn
    /// </summary>
    private void ToggleSelection()
    {
        SetSelected(!isSelected);
        OnSelectionChanged?.Invoke(armorUUID, isSelected);
    }

    /// <summary>
    /// Set trạng thái chọn trực tiếp (dùng cho nút Thu hồi)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (checkMark != null) checkMark.SetActive(isSelected);
    }
}
