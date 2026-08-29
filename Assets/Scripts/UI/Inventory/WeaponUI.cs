using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponUI : InventoryItemUI
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgrades;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject selected;

    private bool isRecallMode = false;
    private bool isUnselectable = false; // Equipped or Legendary
    private bool isSelected = false;
    private int salvageEssenceValue = 0;

    public bool IsSelected => isSelected;
    public bool IsUnselectable => isUnselectable;
    public bool IsRecallMode => isRecallMode;
    public int SalvageEssenceValue => salvageEssenceValue;

    public Action<string, bool> OnRecallSelectionToggled;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber, bool unselectable = false)
    {
        base.Setup(id, rare, icon, background);
        if (txtLevel != null) txtLevel.text = level.ToString();
        if (upgrades != null) upgrades.UpdateUI(upgradeNumber);

        isUnselectable = unselectable || rare == Rare.Legendary;
        salvageEssenceValue = Utility.GetWeaponSalvageEssence(rare, level);
        isRecallMode = false;
        isSelected = false;

        if (overlay != null && overlay.activeSelf) overlay.SetActive(false);
        if (selected != null && selected.activeSelf) selected.SetActive(false);
    }

    public void SetRecallMode(bool inRecallMode)
    {
        isRecallMode = inRecallMode;

        if (isRecallMode)
        {
            if (isUnselectable)
            {
                if (overlay != null && !overlay.activeSelf) overlay.SetActive(true);
                if (selected != null && selected.activeSelf) selected.SetActive(false);
                isSelected = false;
            }
            else
            {
                if (overlay != null && overlay.activeSelf) overlay.SetActive(false);
                if (selected != null && selected.activeSelf != isSelected) selected.SetActive(isSelected);
            }
        }
        else
        {
            isSelected = false;
            if (overlay != null && overlay.activeSelf) overlay.SetActive(false);
            if (selected != null && selected.activeSelf) selected.SetActive(false);
        }
    }

    public void SetRecallMode(bool inRecallMode, bool unselectable)
    {
        isUnselectable = unselectable;
        SetRecallMode(inRecallMode);
    }

    public void SetSelected(bool isSel)
    {
        isSelected = isSel;
        if (selected != null) selected.SetActive(isSelected);
    }

    public void SetOverlay(bool active)
    {
        if (overlay != null) overlay.SetActive(active);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!CanClick) return;

        if (isRecallMode)
        {
            // Các weapon đang được equip hoặc phẩm chất Legendary active overlay biểu thị không thể click
            if (isUnselectable)
            {
                return;
            }

            // Còn lại có thể click được và cũng có thể click lần nữa để hủy (toggle)
            isSelected = !isSelected;
            SetSelected(isSelected);
            OnRecallSelectionToggled?.Invoke(id, isSelected);
        }
        else
        {
            base.OnPointerClick(eventData);
        }
    }
}
