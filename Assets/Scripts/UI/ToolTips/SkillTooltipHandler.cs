using UnityEngine;
using VContainer;

/// <summary>
/// Gắn lên SkillCharacterUI để kết nối TooltipTrigger với SkillTooltipUI.
/// Khi trigger hover/long-press → tạo SkillTooltipData → fire UIEvent.
/// </summary>
[RequireComponent(typeof(TooltipTrigger))]
public class SkillTooltipHandler : MonoBehaviour
{
    [SerializeField] private SkillCharacter _skillType;

    [Inject] private GameDataBase _gameDataBase;
    [Inject] private PlayerCharacterManager _playerCharacterManager;

    private TooltipTrigger _trigger;
    private string _characterID;

    private void Awake()
    {
        _trigger = GetComponent<TooltipTrigger>();
    }

    private void OnEnable()
    {
        _trigger.OnTooltipShow += HandleShow;
        _trigger.OnTooltipHide += HandleHide;
    }

    private void OnDisable()
    {
        _trigger.OnTooltipShow -= HandleShow;
        _trigger.OnTooltipHide -= HandleHide;
    }

    /// <summary>
    /// Thiết lập character ID cho context.
    /// Gọi bởi CharacterCardInfo hoặc bất kỳ parent nào quản lý skill UI.
    /// </summary>
    public void SetCharacterID(string id)
    {
        _characterID = id;
    }

    private void HandleShow()
    {
        if (string.IsNullOrEmpty(_characterID)) return;

        var config = _gameDataBase.GetCharacterConfig(_characterID);
        if (config == null || !config.Skills.ContainsKey(_skillType)) return;

        var profile = _playerCharacterManager.GetCharacter(_characterID);
        int starUp = profile != null ? profile.SaveData.StarUp : 0;
        int enhancementLevel = Utility.GetSkillEnhancementLevel(_skillType, starUp);

        SkillComponent skillComp = config.Skills[_skillType];

        // Lấy icon tương ứng
        Sprite icon = _skillType switch
        {
            SkillCharacter.Base     => config.BaseSkillIcon,
            SkillCharacter.Major    => config.MajorSkillIcon,
            SkillCharacter.Ultimate => config.UltimateSkillIcon,
            _ => null
        };

        // Lấy các chỉ số skill theo enhancement level
        float damageMultiplier = skillComp.GetDamageMultiplier(enhancementLevel);
        int maxCooldown = skillComp.GetMaxCooldown(enhancementLevel);

        // 1. Lấy giá trị Passive nếu có
        float passiveValue = 0f;
        if (!string.IsNullOrEmpty(skillComp.PassiveID))
        {
            var passiveConfig = _gameDataBase.GetPassiveConfig(skillComp.PassiveID);
            if (passiveConfig != null)
            {
                int index = Mathf.Max(0, enhancementLevel - 1);
                if (passiveConfig.StaticModifiers != null && passiveConfig.StaticModifiers.Count > 0 && passiveConfig.StaticModifiers[0].ModifyByUpgrade != null && passiveConfig.StaticModifiers[0].ModifyByUpgrade.Count > 0)
                {
                    var list = passiveConfig.StaticModifiers[0].ModifyByUpgrade;
                    passiveValue = list[Mathf.Min(index, list.Count - 1)];
                }
                else if (passiveConfig.CombatEvents != null && passiveConfig.CombatEvents.Count > 0 && passiveConfig.CombatEvents[0].ModifyByUpgrade != null && passiveConfig.CombatEvents[0].ModifyByUpgrade.Count > 0)
                {
                    var list = passiveConfig.CombatEvents[0].ModifyByUpgrade;
                    passiveValue = list[Mathf.Min(index, list.Count - 1)];
                }
            }
        }

        // 2. Lấy giá trị Effect nếu có
        float effectValue = 0f;
        if (!string.IsNullOrEmpty(skillComp.EffectID))
        {
            var effectConfig = _gameDataBase.GetEffectConfig(skillComp.EffectID);
            if (effectConfig != null)
            {
                effectValue = effectConfig.Value;
            }
        }

        // Format description: {0} = DamageMultiplier%, {1} = Passive Value, {2} = Effect Value
        string rawDescription = LocalizationManager.Instance.GetLocalizedValue(skillComp.Description);
        string formattedDescription;
        try
        {
            formattedDescription = string.Format(rawDescription,
                (damageMultiplier * 100f).ToString("F0"),   // {0} DamageMultiplier %
                passiveValue,                               // {1} Passive Value
                effectValue                                 // {2} Effect Value
            );
        }
        catch (System.FormatException)
        {
            formattedDescription = rawDescription;
        }

        var data = new SkillTooltipData
        {
            SkillName = LocalizationManager.Instance.GetLocalizedValue(skillComp.Name),
            SkillDescription = formattedDescription,
            SkillType = _skillType,
            Category = skillComp.Type,
            DamageMultiplier = damageMultiplier,
            MaxCooldown = maxCooldown,
            EnhancementLevel = enhancementLevel,
            Icon = icon
        };

        // Lấy đúng camera dựa trên Canvas render mode
        Canvas handlerCanvas = GetComponentInParent<Canvas>();
        Camera cam = (handlerCanvas != null && handlerCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? handlerCanvas.worldCamera
            : null;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, transform.position);

        UIEvent.OnShowSkillTooltip?.Invoke(data, screenPos);
    }

    private void HandleHide()
    {
        UIEvent.OnHideSkillTooltip?.Invoke();
    }
}
