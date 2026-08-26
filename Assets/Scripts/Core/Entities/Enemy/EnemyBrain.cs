using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tech.Composite;
using Cysharp.Threading.Tasks;

public class EnemyDecision
{
    public SkillCharacter SkillType;
    public Entity Target;
}

public abstract class EnemyBrain : CoreComponent
{
    protected Entity _entity;

    public override void LoadComponent()
    {
        _entity = core as Entity;
    }

    public List<Entity> GetAliveTargets(List<Entity> targets)
    {
        if (targets == null || targets.Count == 0) return new List<Entity>();
        return targets.Where(p => p != null && p.GetCoreComponent<EntityStats>() != null && !p.GetCoreComponent<EntityStats>().IsDead).ToList();
    }

    public Entity GetLowestHpTarget(List<Entity> targets)
    {
        var alive = GetAliveTargets(targets);
        if (alive.Count == 0) return null;
        return alive.OrderBy(p => p.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value).FirstOrDefault();
    }

    public Entity GetHighestAtkTarget(List<Entity> targets)
    {
        var alive = GetAliveTargets(targets);
        if (alive.Count == 0) return null;
        return alive.OrderByDescending(p => p.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
    }

    /// <summary>
    /// Tìm mục tiêu cho kỹ năng Khống Chế Cứng (Choáng / Đóng Băng).
    /// Ưu tiên kẻ địch ĐANG HOẠT ĐỘNG (chưa bị Choáng/Đóng Băng), chọn kẻ có ATK cao nhất.
    /// Trả về null nếu TẤT CẢ mục tiêu đều đã bị khống chế cứng.
    /// </summary>
    public Entity GetBestTargetForHardCC(List<Entity> targets)
    {
        var alive = GetAliveTargets(targets);
        if (alive.Count == 0) return null;

        var activeTargets = alive.Where(e => {
            var stats = e.GetCoreComponent<EntityStats>();
            return stats != null && stats.CanTakeTurn();
        }).ToList();

        if (activeTargets.Count > 0)
        {
            return activeTargets.OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Tìm mục tiêu cho kỹ năng Câm Lặng.
    /// Ưu tiên kẻ địch chưa bị Câm Lặng và chưa bị Khống Chế Cứng.
    /// </summary>
    public Entity GetBestTargetForSilence(List<Entity> targets)
    {
        var alive = GetAliveTargets(targets);
        if (alive.Count == 0) return null;

        var validTargets = alive.Where(e => {
            var stats = e.GetCoreComponent<EntityStats>();
            return stats != null && stats.CanTakeTurn() && !stats.IsSilenced();
        }).ToList();

        if (validTargets.Count > 0)
        {
            return validTargets.OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra kỹ năng có phải là Buff Chỉ Số hoặc Buff Khiên cho Bản Thân / Đồng Đội hay không.
    /// TUYỆT ĐỐI CHỈ ÁP DỤNG cho kỹ năng nhắm vào Đồng Đội / Bản Thân, KHÔNG áp dụng cho đòn đánh kẻ địch!
    /// </summary>
    public static bool IsSelfOrAllyBuffSkill(SkillRuntime skill)
    {
        if (skill == null) return false;
        var data = skill.GetSkillData();
        if (data == null) return false;

        // Phải là chiêu có TargetType hướng vào Bản thân hoặc Đồng đội
        bool isFriendlyTarget = data.TargetType == SkillTargetType.Self 
            || data.TargetType == SkillTargetType.SingleAlly 
            || data.TargetType == SkillTargetType.AllAllies 
            || data.TargetType == SkillTargetType.SameRowAllies;

        if (!isFriendlyTarget) return false;

        if (skill is BuffShieldSkill || skill is StatModifierSkill) return true;
        if (data.Effect != null && data.Effect.IsBuff()) return true;
        if (data.SkillType == SkillType.NonAttackSkill && !(skill is HealingSkill)) return true;

        return false;
    }

    /// <summary>
    /// Kiểm tra kỹ năng có phải là Kỹ năng Hồi Máu hay không.
    /// </summary>
    public static bool IsHealingSkill(SkillRuntime skill)
    {
        if (skill == null) return false;
        var data = skill.GetSkillData();
        if (data == null) return false;

        if (skill is HealingSkill) return true;
        if (data.ID != null && (data.ID.Contains("Heal") || data.ID.Contains("Recovery"))) return true;
        return false;
    }

    /// <summary>
    /// Tìm đồng đội có tỉ lệ máu thấp nhất dưới ngưỡng quy định (mặc định 50% HP).
    /// </summary>
    public static Entity GetLowestHpAllyBelowThreshold(List<Entity> allies, float threshold = 0.5f)
    {
        if (allies == null || allies.Count == 0) return null;
        var lowHpAllies = allies.Where(a => {
            if (a == null) return false;
            var stats = a.GetCoreComponent<EntityStats>();
            if (stats == null || stats.IsDead) return false;
            var hpAttr = stats.GetAttribute(AttributeType.Hp);
            return hpAttr != null && hpAttr.GetPercent() < threshold;
        }).ToList();

        if (lowHpAllies.Count > 0)
        {
            return lowHpAllies.OrderBy(a => a.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).GetPercent()).FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Kiểm tra kỹ năng có phải là Kỹ năng Áp Đặt Dấu Ấn / Hiệu Ứng Bất Lợi Đặc Biệt (Mark / Debuff Setup) hay không.
    /// </summary>
    public static bool IsMarkOrSetupSkill(SkillRuntime skill)
    {
        if (skill == null) return false;
        var data = skill.GetSkillData();
        if (data == null || data.Effect == null) return false;

        return data.Effect.Type == EffectType.SilverMark || data.Effect.Type == EffectType.StatDebuff;
    }

    public abstract UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam);
}
