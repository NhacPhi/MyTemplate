using UnityEngine;
using TMPro;
using System;

public class ShopCategoryToggle : ToggleBase
{
    private string categoryId;

    private Action<string> onCategorySelected;

    public void Setup(string categoryId, string nameKey, Action<string> onSelectedCallback)
    {
        this.categoryId = categoryId;
        this.onCategorySelected = onSelectedCallback;
        
        // Bạn có thể xử lý nameKey tại đây nếu cần đổi ID cho LocalizedText,
        // nếu LocalizedText tự lo mọi thứ thì biến nameKey có thể bỏ đi.
    }

    public override void OnSelected(bool isOn)
    {
        base.OnSelected(isOn);
        if (isOn)
        {
            onCategorySelected?.Invoke(categoryId);
        }
    }
}
