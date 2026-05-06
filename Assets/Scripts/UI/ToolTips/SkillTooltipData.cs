using System;

/// <summary>
/// Data payload cho Skill Tooltip.
/// Chứa toàn bộ thông tin cần hiển thị trên tooltip.
/// </summary>
[Serializable]
public class SkillTooltipData
{
    public string SkillName;
    public string SkillDescription;
    public SkillCharacter SkillType;
    public SkillType Category;
    public float DamageMultiplier;
    public int MaxCooldown;
    public int EnhancementLevel;
    public UnityEngine.Sprite Icon;
}
