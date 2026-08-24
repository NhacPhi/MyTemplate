using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossBrain : EnemyBrain
{
    public override async UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam)
    {
        var aliveTargets = GetAliveTargets(playerTeam);
        if (aliveTargets.Count == 0)
        {
            return new EnemyDecision { SkillType = SkillCharacter.Base, Target = null };
        }

        var skillManager = _entity != null ? _entity.GetCoreComponent<EntitySkill>() : null;
        var casterStats = _entity != null ? _entity.GetCoreComponent<EntityStats>() : null;
        bool isSilenced = casterStats != null && casterStats.IsSilenced();

        var bossAllies = BattleManager.Instance != null 
            ? BattleManager.Instance.GetEntitiesByTeam(_entity.Team) 
            : new List<Entity>() { _entity };

        // -------------------------------------------------------------
        // BƯỚC 1: ƯU TIÊN SỐ 1 - KỸ NĂNG BUFF CHỈ SỐ / KHIÊN HỘ THỂ
        // -------------------------------------------------------------
        if (!isSilenced && skillManager != null)
        {
            // Kiểm tra Ultimate có phải là Buff/Shield không
            if (skillManager.IsSkillReady(SkillCharacter.Ultimate) && skillManager.Skills.ContainsKey(SkillCharacter.Ultimate))
            {
                var ultRuntime = skillManager.Skills[SkillCharacter.Ultimate];
                if (IsSelfOrAllyBuffSkill(ultRuntime))
                {
                    return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = _entity };
                }
            }

            // Kiểm tra Major có phải là Buff/Shield không
            if (skillManager.IsSkillReady(SkillCharacter.Major) && skillManager.Skills.ContainsKey(SkillCharacter.Major))
            {
                var majorRuntime = skillManager.Skills[SkillCharacter.Major];
                if (IsSelfOrAllyBuffSkill(majorRuntime))
                {
                    return new EnemyDecision { SkillType = SkillCharacter.Major, Target = _entity };
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 2: ƯU TIÊN SỐ 2 - KỸ NĂNG HỒI MÁU (KHI CÓ ĐỒNG ĐỘI < 50% HP)
        // -------------------------------------------------------------
        if (!isSilenced && skillManager != null)
        {
            Entity lowHpAlly = GetLowestHpAllyBelowThreshold(bossAllies, 0.5f);
            if (lowHpAlly != null)
            {
                if (skillManager.IsSkillReady(SkillCharacter.Ultimate) && skillManager.Skills.ContainsKey(SkillCharacter.Ultimate) && IsHealingSkill(skillManager.Skills[SkillCharacter.Ultimate]))
                {
                    return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = lowHpAlly };
                }
                if (skillManager.IsSkillReady(SkillCharacter.Major) && skillManager.Skills.ContainsKey(SkillCharacter.Major) && IsHealingSkill(skillManager.Skills[SkillCharacter.Major]))
                {
                    return new EnemyDecision { SkillType = SkillCharacter.Major, Target = lowHpAlly };
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 3: ƯU TIÊN SỐ 3 - TUYỆT KỸ ULTIMATE TẤN CÔNG / KHỐNG CHẾ
        // -------------------------------------------------------------
        if (!isSilenced && skillManager != null && skillManager.IsSkillReady(SkillCharacter.Ultimate) && skillManager.Skills.ContainsKey(SkillCharacter.Ultimate))
        {
            var ultRuntime = skillManager.Skills[SkillCharacter.Ultimate];
            var ultData = ultRuntime.GetSkillData();

            // Nếu không phải là chiêu Heal đang chờ (vì Heal đã kiểm tra ở Bước 2)
            if (!IsHealingSkill(ultRuntime))
            {
                if (ultData != null && ultData.Effect != null && (ultData.Effect.Type == EffectType.Stun || ultData.Effect.Type == EffectType.Frozen))
                {
                    if (ultData.TargetType == SkillTargetType.AllEnemies || ultData.TargetType == SkillTargetType.EnemyRow)
                    {
                        bool hasActiveEnemy = aliveTargets.Any(e => e.GetCoreComponent<EntityStats>().CanTakeTurn());
                        if (hasActiveEnemy)
                        {
                            return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = aliveTargets[0] };
                        }
                    }
                    else
                    {
                        Entity ccTarget = GetBestTargetForHardCC(aliveTargets);
                        if (ccTarget != null)
                        {
                            return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = ccTarget };
                        }
                    }
                }
                else if (ultData != null && ultData.Effect != null && ultData.Effect.Type == EffectType.Silence)
                {
                    Entity silTarget = GetBestTargetForSilence(aliveTargets);
                    if (silTarget != null)
                    {
                        return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = silTarget };
                    }
                }
                else
                {
                    // Sát thương dồn vào kẻ địch thấp máu nhất để kết liễu
                    Entity finishTarget = GetLowestHpTarget(aliveTargets);
                    return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = finishTarget };
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 4: ƯU TIÊN SỐ 4 - KỸ NĂNG MAJOR TẤN CÔNG / KHỐNG CHẾ
        // -------------------------------------------------------------
        if (!isSilenced && skillManager != null && skillManager.IsSkillReady(SkillCharacter.Major) && skillManager.Skills.ContainsKey(SkillCharacter.Major))
        {
            var majorRuntime = skillManager.Skills[SkillCharacter.Major];
            var majorData = majorRuntime.GetSkillData();

            if (!IsHealingSkill(majorRuntime))
            {
                if (majorData != null && majorData.Effect != null && (majorData.Effect.Type == EffectType.Stun || majorData.Effect.Type == EffectType.Frozen))
                {
                    Entity ccTarget = GetBestTargetForHardCC(aliveTargets);
                    if (ccTarget != null)
                    {
                        return new EnemyDecision { SkillType = SkillCharacter.Major, Target = ccTarget };
                    }
                }
                else if (majorData != null && majorData.Effect != null && majorData.Effect.Type == EffectType.Silence)
                {
                    Entity silTarget = GetBestTargetForSilence(aliveTargets);
                    if (silTarget != null)
                    {
                        return new EnemyDecision { SkillType = SkillCharacter.Major, Target = silTarget };
                    }
                }
                else
                {
                    Entity target = GetLowestHpTarget(aliveTargets);
                    return new EnemyDecision { SkillType = SkillCharacter.Major, Target = target };
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 5: ĐÒN ĐÁNH CƠ BẢN (BASE ATTACK) - KẾT LIỄU MỤC TIÊU YẾU NHẤT
        // -------------------------------------------------------------
        Entity executeTarget = GetLowestHpTarget(aliveTargets);
        return new EnemyDecision { SkillType = SkillCharacter.Base, Target = executeTarget };
    }
}
