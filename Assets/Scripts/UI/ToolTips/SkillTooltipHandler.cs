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
        if (_trigger == null)
            _trigger = GetComponent<TooltipTrigger>();

        if (_trigger != null)
        {
            _trigger.OnTooltipShow -= HandleShow;
            _trigger.OnTooltipShow += HandleShow;
            _trigger.OnTooltipHide -= HandleHide;
            _trigger.OnTooltipHide += HandleHide;
        }
    }

    private void OnDisable()
    {
        if (_trigger != null)
        {
            _trigger.OnTooltipShow -= HandleShow;
            _trigger.OnTooltipHide -= HandleHide;
        }
    }

    /// <summary>
    /// Thiết lập skill type cho handler.
    /// </summary>
    public void SetSkillType(SkillCharacter skillType)
    {
        _skillType = skillType;
    }

    /// <summary>
    /// Thiết lập character ID cho context.
    /// Gọi bởi CharacterCardInfo, ToggleSkillCharacterUI, SkillBossUI hoặc bất kỳ parent nào quản lý skill UI.
    /// </summary>
    public void SetCharacterID(string id)
    {
        _characterID = id;
    }

    private void EnsureDependencies()
    {
        if (_gameDataBase == null || _playerCharacterManager == null)
        {
            var rootScope = FindFirstObjectByType<RootScope>();
            if (rootScope != null && rootScope.Container != null)
            {
                if (_gameDataBase == null)
                {
                    try { _gameDataBase = rootScope.Container.Resolve<GameDataBase>(); } catch { }
                }
                if (_playerCharacterManager == null)
                {
                    try { _playerCharacterManager = rootScope.Container.Resolve<PlayerCharacterManager>(); } catch { }
                }
            }
        }
    }

    private void HandleShow()
    {
        Debug.Log($"[SkillTooltipHandler] HandleShow called. CharacterID: {_characterID}, SkillType: {_skillType}");
        if (string.IsNullOrEmpty(_characterID)) return;

        EnsureDependencies();

        if (_gameDataBase == null)
        {
            Debug.LogWarning("[SkillTooltipHandler] GameDataBase is null!");
            return;
        }

        var config = _gameDataBase.GetCharacterConfig(_characterID);
        if (config == null) 
        {
            Debug.LogWarning($"[SkillTooltipHandler] CharacterConfig is null for ID: {_characterID}!");
            return;
        }
        if (config.Skills == null || !config.Skills.ContainsKey(_skillType))
        {
            Debug.LogWarning($"[SkillTooltipHandler] CharacterConfig does not contain skill type: {_skillType}");
            return;
        }

        int starUp = 0;
        if (_playerCharacterManager != null)
        {
            var profile = _playerCharacterManager.GetCharacter(_characterID);
            if (profile != null && profile.SaveData != null)
            {
                starUp = profile.SaveData.StarUp;
            }
        }
        
        int enhancementLevel = Utility.GetSkillEnhancementLevel(_skillType, starUp);

        SkillComponent skillComp = config.Skills[_skillType];
        if (skillComp == null) return;

        // Lấy icon tương ứng (ưu tiên IconSprite đã nạp trong SkillComponent, fallback sang config property)
        Sprite icon = skillComp.IconSprite;
        if (icon == null)
        {
            icon = _skillType switch
            {
                SkillCharacter.Base     => config.BaseSkillIcon,
                SkillCharacter.Major    => config.MajorSkillIcon,
                SkillCharacter.Ultimate => config.UltimateSkillIcon,
                _ => null
            };
        }

        // Lấy các chỉ số skill theo enhancement level
        float damageMultiplier = skillComp.GetDamageMultiplier(enhancementLevel);
        int maxCooldown = skillComp.GetMaxCooldown(enhancementLevel);

        // 1. Lấy giá trị Passive nếu có
        float passiveValue = 0f;
        float passiveParam = 0f;
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
                else if (passiveConfig.CombatEvents != null && passiveConfig.CombatEvents.Count > 0)
                {
                    var ce = passiveConfig.CombatEvents[0];
                    if (ce.ModifyByUpgrade != null && ce.ModifyByUpgrade.Count > 0)
                    {
                        passiveValue = ce.ModifyByUpgrade[Mathf.Min(index, ce.ModifyByUpgrade.Count - 1)];
                    }
                    passiveParam = ce.EffectParam > 0 ? ce.EffectParam : 100f;
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
            // Tự động sửa các lỗi gõ nhầm dấu ngoặc vuông và escape ký tự xuống dòng
            rawDescription = rawDescription
                .Replace("{0]", "{0}")
                .Replace("{1]", "{1}")
                .Replace("{2]", "{2}")
                .Replace("{3]", "{3}")
                .Replace("\\n", "\n");

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
                    // Với skill thông thường / có passive (như BullDemonKing_B):
                    // {0} = DamageMultiplier % (100%)
                    // {1} = Passive Value (Xác suất %: 50%, 75%, 100%)
                    // {2} = Passive Param (Sát thương phản đòn: 100%)
                    // {3} = Effect Duration
                    arg1 = passiveValue > 0 ? passiveValue.ToString("F0") : (effectValue > 0 ? effectValue.ToString("F0") : "");
                    arg2 = passiveParam > 0 ? passiveParam.ToString("F0") : (effectValue > 0 ? effectValue.ToString("F0") : "");
                    arg3 = effectDuration > 0 ? effectDuration.ToString() : "";
                }

                formattedDescription = string.Format(rawDescription, arg0, arg1, arg2, arg3);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SkillTooltipHandler] Format description failed for {skillComp.ID}: {ex.Message}");
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

        RectTransform triggerRect = transform as RectTransform;

        // Lấy đúng camera dựa trên Canvas render mode
        Canvas handlerCanvas = GetComponentInParent<Canvas>();
        Camera cam = (handlerCanvas != null && handlerCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? handlerCanvas.worldCamera
            : null;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, transform.position);

        Debug.Log($"[SkillTooltipHandler] Invoking OnShowSkillTooltip for {_skillType}");

        if (UIEvent.OnShowSkillTooltipWithRect == null && UIEvent.OnShowSkillTooltip == null)
        {
            // Tự động tìm và kích hoạt SkillTooltipUI trong scene nếu ban đầu nó bị inactive
            var tooltipUI = FindFirstObjectByType<SkillTooltipUI>(FindObjectsInactive.Include);
            if (tooltipUI != null)
            {
                tooltipUI.gameObject.SetActive(true);
                if (triggerRect != null)
                {
                    tooltipUI.Show(data, triggerRect);
                }
                else
                {
                    tooltipUI.Show(data, screenPos);
                }
                return;
            }
            else
            {
                Debug.LogError("[SkillTooltipHandler] UIEvent.OnShowSkillTooltip is NULL! Không tìm thấy SkillTooltipUI trong scene. Vui lòng đảm bảo prefab SkillToolTips đã được thêm vào Canvas của Scene!");
                return;
            }
        }

        if (triggerRect != null && UIEvent.OnShowSkillTooltipWithRect != null)
        {
            UIEvent.OnShowSkillTooltipWithRect.Invoke(data, triggerRect);
        }
        else
        {
            UIEvent.OnShowSkillTooltip?.Invoke(data, screenPos);
        }
    }

    private void HandleHide()
    {
        UIEvent.OnHideSkillTooltip?.Invoke();
    }
}
