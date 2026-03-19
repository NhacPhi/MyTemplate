using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SkillBossUI : MonoBehaviour
{
    [SerializeField] private SkillCharacter _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtNumberCooldown;
    [SerializeField] private Image _imgCooldown;

    public void SetIconSkill(Sprite sprite)
    {
        _icon.sprite = sprite;
    }
    public void UpdateSkillUI(SkillComponent skillData, int currentCooldown)
    {
        bool isReady = currentCooldown <= 0;

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

                float maxCD = skillData.MaxCooldown > 0 ? skillData.MaxCooldown : 1f;


                _imgCooldown.fillAmount = (float)currentCooldown / maxCD;
            }
        }
    }
}
