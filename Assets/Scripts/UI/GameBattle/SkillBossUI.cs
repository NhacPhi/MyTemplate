using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SkillBossUI : MonoBehaviour
{
    [SerializeField] private SkillCharacter _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtNumberCooldown;
    [SerializeField] private Image _imgCooldown;

    private SkillTooltipHandler _tooltipHandler;

    private void Awake()
    {
        InitTooltipHandler();
    }

    private void InitTooltipHandler()
    {
        if (_tooltipHandler == null)
        {
            _tooltipHandler = GetComponent<SkillTooltipHandler>();
            if (_tooltipHandler == null)
            {
                _tooltipHandler = gameObject.AddComponent<SkillTooltipHandler>();
            }
            _tooltipHandler.SetSkillType(_type);
        }
    }

    public void SetCharacterID(string characterID)
    {
        InitTooltipHandler();
        if (_tooltipHandler != null)
        {
            _tooltipHandler.SetSkillType(_type);
            _tooltipHandler.SetCharacterID(characterID);
        }
    }

    public void SetIconSkill(Sprite sprite)
    {
        if (_icon != null && sprite != null)
        {
            _icon.sprite = sprite;
        }
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

                int maxCooldown = skillData.GetMaxCooldown(0);
                float maxCD = maxCooldown > 0 ? maxCooldown : 1f;


                _imgCooldown.fillAmount = (float)currentCooldown / maxCD;
            }
        }
    }
}
