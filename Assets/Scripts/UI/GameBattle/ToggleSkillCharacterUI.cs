using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleSkillCharacterUI : ToggleBase
{
    [SerializeField] private SkillCharacter _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtNumberCooldown;
    [SerializeField] private Image _imgCooldown;

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

    public void UpdateSkillUI(SkillComponent skillData, int currentCooldown)
    {
        bool isReady = currentCooldown <= 0;

        toggle.interactable = isReady;

        if (isReady)
        {
            _txtNumberCooldown.text = "";

            if (_imgCooldown != null)
            {
                _imgCooldown.fillAmount = 0f;
            }
        }
        else
        {
            _txtNumberCooldown.text = currentCooldown.ToString();

            if (_imgCooldown != null)
            {

                int maxCooldown = skillData.GetMaxCooldown(0);
                float maxCD = maxCooldown > 0 ? maxCooldown : 1f;


                _imgCooldown.fillAmount = (float)currentCooldown / maxCD;
            }
        }
    }
}
