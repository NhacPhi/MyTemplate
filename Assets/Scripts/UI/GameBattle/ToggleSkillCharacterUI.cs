using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleSkillCharacterUI : ToggleBase
{
    [SerializeField] private SkillCharacter _type;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtNumberCooldown;
    [SerializeField] private Image _imgCooldown;

    private SkillTooltipHandler _tooltipHandler;

    protected override void Awake()
    {
        base.Awake();
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

    public override void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnChooseSkillCharacter?.Invoke(_type);
        }
    }

    public void UpdateSkillUI(SkillComponent skillData, int currentCooldown)
    {
        bool isReady = currentCooldown <= 0;

        if (Toggle != null)
        {
            Toggle.interactable = isReady;
        }

        if (_txtNumberCooldown != null)
        {
            _txtNumberCooldown.text = isReady ? "" : currentCooldown.ToString();
        }

        if (_imgCooldown != null)
        {
            if (isReady)
            {
                _imgCooldown.fillAmount = 0f;
            }
            else
            {
                int maxCooldown = skillData != null ? skillData.GetMaxCooldown(0) : 1;
                float maxCD = maxCooldown > 0 ? maxCooldown : 1f;
                _imgCooldown.fillAmount = (float)currentCooldown / maxCD;
            }
        }
    }
}
