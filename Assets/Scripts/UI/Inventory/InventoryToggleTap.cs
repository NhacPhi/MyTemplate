using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class InventoryToggleTap : MonoBehaviour
{
    [SerializeField] private ItemType type;
    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnSelected);
    }
    public ItemType Type => type;

    public void Setup(ToggleGroup group, ItemType type)
    {
        toggle.group = group;
        this.type = type;
    }

    private void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnSelectToggleTap?.Invoke(type);
        }
    }
}
