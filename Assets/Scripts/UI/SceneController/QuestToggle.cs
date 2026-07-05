using UnityEngine;
using TMPro;

public class QuestToggle : ToggleBase
{
    [SerializeField] private TextMeshProUGUI txtLabel;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.gray;

    public override void OnSelected(bool isOn)
    {
        base.OnSelected(isOn);
        
        if (txtLabel != null)
        {
            txtLabel.color = isOn ? selectedColor : unselectedColor;
        }
    }
}
