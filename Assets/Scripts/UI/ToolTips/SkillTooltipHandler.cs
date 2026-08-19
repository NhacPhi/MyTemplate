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
        Debug.Log($"[SkillTooltipHandler] HandleShow called. CharacterID: {_characterID}, SkillType: {_skillType}");
        if (string.IsNullOrEmpty(_characterID)) return;

        var config = _gameDataBase.GetCharacterConfig(_characterID);
        if (config == null) 
        {
            Debug.LogWarning("[SkillTooltipHandler] CharacterConfig is null!");
            return;
        }
        if (!config.Skills.ContainsKey(_skillType))
        {
            Debug.LogWarning($"[SkillTooltipHandler] CharacterConfig does not contain skill type: {_skillType}");
            return;
        }

        var profile = _playerCharacterManager.GetCharacter(_characterID);
        if (profile == null)
        {
            Debug.LogWarning("[SkillTooltipHandler] Character profile is null!");
            return;
        }
        
        int starUp = profile.SaveData.StarUp;
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

        // 2. Lấy giá trị Effect & Duration nếu có
        float effectValue = 0f;
        int effectDuration = 0;
        if (!string.IsNullOrEmpty(skillComp.EffectID))
        {
            var effectConfig = _gameDataBase.GetEffectConfig(skillComp.EffectID);
            if (effectConfig != null)
            {
                effectValue = effectConfig.Value;
                effectDuration = effectConfig.Duration;
            }
        }

        // Format description thông minh theo loại kỹ năng và effect
        string rawDescription = LocalizationManager.Instance.GetLocalizedValue(skillComp.Description);
        string formattedDescription = rawDescription;
        if (!string.IsNullOrEmpty(rawDescription))
        {
            try
            {
                object arg0 = (damageMultiplier * 100f).ToString("F0");
                object arg1 = "";
                object arg2 = "";
                object arg3 = "";

                if (skillComp.Skill == Skill.StatModifier || skillComp.TargetType == SkillTargetType.Self)
                {
                    // Với skill buff (như Ngưu Ma Vương M, Sa Tăng M):
                    // {0} = % Buff (Lấy từ DamageMultiplier hoặc Effect Value)
                    // {1} = Duration (Số hiệp)
                    // {2} = Secondary effect value (nếu có)
                    arg0 = (damageMultiplier > 0 && damageMultiplier != 1f) 
                        ? (damageMultiplier * 100f).ToString("F0") 
                        : (effectValue > 0 ? effectValue.ToString("F0") : (damageMultiplier * 100f).ToString("F0"));
                    arg1 = effectDuration > 0 ? effectDuration : (passiveValue > 0 ? passiveValue : 1);
                    arg2 = effectValue > 0 ? effectValue : passiveValue;
                    arg3 = effectDuration;
                }
                else if (!string.IsNullOrEmpty(skillComp.EffectID))
                {
                    // Với skill tấn công có đính kèm Effect (như Ngưu Ma Vương U Poison, Đường Tăng Choáng, ...):
                    // {0} = DamageMultiplier %
                    // {1} = Effect Value (hoặc Passive Value)
                    // {2} = Effect Duration
                    // {3} = Passive Value
                    arg1 = effectValue > 0 ? effectValue.ToString("F0") : (passiveValue > 0 ? passiveValue.ToString("F0") : effectDuration.ToString());
                    arg2 = effectDuration > 0 ? effectDuration : passiveValue;
                    arg3 = passiveValue;
                }
                else
                {
                    // Với skill thông thường / có passive:
                    // {0} = DamageMultiplier %
                    // {1} = Passive Value
                    // {2} = Effect Value
                    // {3} = Effect Duration
                    arg1 = passiveValue;
                    arg2 = effectValue;
                    arg3 = effectDuration;
                }

                formattedDescription = string.Format(rawDescription, arg0, arg1, arg2, arg3);
            }
            catch (System.FormatException)
            {
                formattedDescription = rawDescription;
            }
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

        Debug.Log($"[SkillTooltipHandler] Invoking OnShowSkillTooltip for {_skillType} at {screenPos}");
        if (UIEvent.OnShowSkillTooltip == null)
        {
            Debug.LogError("[SkillTooltipHandler] UIEvent.OnShowSkillTooltip is NULL! No one is listening. (Check if SkillTooltipUI is active in the scene!)");
        }
        UIEvent.OnShowSkillTooltip?.Invoke(data, screenPos);
    }

    private void HandleHide()
    {
        UIEvent.OnHideSkillTooltip?.Invoke();
    }
}
