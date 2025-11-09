using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class InventoryToggleTap : ToggleBase
{
    [SerializeField] private ItemType type;

    public ItemType Type => type;

    public void Setup(ToggleGroup group, ItemType type)
    {
        toggle.group = group;
        this.type = type;
    }

    public override void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnSelectToggleInventoryTap?.Invoke(type);
        }
    }
}
