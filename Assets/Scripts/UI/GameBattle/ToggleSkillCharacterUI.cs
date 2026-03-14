using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleSkillCharacterUI : ToggleBase
{
    [SerializeField] private SkillCharacter _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtNumberCooldown;
    [SerializeField] private Image _imgCooldown;
 
    private bool _isInteractable = true;

    public void SetIconSkill(Sprite sprite)
    {
        _icon.sprite = sprite;
    }

    public override void OnSelected(bool isOn)
    {
        if(isOn)
        {
            UIEvent.OnChooseSkillCharacter?.Invoke(_type);
        }
    }
}
