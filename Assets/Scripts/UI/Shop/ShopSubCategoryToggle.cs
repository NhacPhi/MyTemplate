using UnityEngine;
using System;

public class ShopSubCategoryToggle : ToggleBase
{
    private string subCategoryId;
    private Action<string> onSubCategorySelected;

    public void Setup(string subCategoryId, string nameKey, Action<string> onSelectedCallback)
    {
        this.subCategoryId = subCategoryId;
        this.onSubCategorySelected = onSelectedCallback;
        
        // LocalizationText component sẽ tự xử lý nameKey giống như với ShopCategoryToggle
    }

    public override void OnSelected(bool isOn)
    {
        base.OnSelected(isOn);
        if (isOn)
        {
            onSubCategorySelected?.Invoke(subCategoryId);
        }
    }
}
