using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetManager 
{
    public List<Entity> GetValidEtitiesByColumnLogic(List<Entity> entities)
    {
        List<Entity> validTargets = new List<Entity>();

        var aliveEnemies = entities.Where(e => !e.GetCoreComponent<EntityStats>().IsDead).ToList();

        var enemiesByColumn = aliveEnemies.GroupBy(e => e.Column);

        foreach (var columnGroup in enemiesByColumn)
        {
            var frontEnemiesInCol = columnGroup.Where(e => e.Row == BattleRow.Front).ToList();

            if (frontEnemiesInCol.Count > 0)
            {
                // Cột này Hàng Trước còn sống -> Chỉ được đánh Hàng Trước
                validTargets.AddRange(frontEnemiesInCol);
            }
            else
            {
                var backEnemiesInCol = columnGroup.Where(e => e.Row == BattleRow.Back).ToList();
                validTargets.AddRange(backEnemiesInCol);
            }
        }

        return validTargets;
    }

    public void HighlightTargets(List<Entity> allEntitiesOnBoard, List<Entity> validTargets)
    {
        foreach (var enemy in allEntitiesOnBoard)
        {
            if (enemy.GetCoreComponent<EntityStats>().IsDead) continue;

            bool isTargetable = validTargets.Contains(enemy);
            enemy.SetTargetableVisual(isTargetable);
        }
    }

    public void ResetTargetVisuals(List<Entity> fullEnemyTeam)
    {
        foreach (var enemy in fullEnemyTeam)
        {
            if (!enemy.GetCoreComponent<EntityStats>().IsDead)
            {
                enemy.ResetTargetVisual();
            }
        }
    }

    public List<Entity> GetValidTargetsForSkill(SkillRuntime skill, Entity caster,List<Entity> allyTeam, List<Entity> enemyTeam)
    {
        var targetType = skill.GetSkillData().TargetType;

        switch (targetType)
        {
            //Single
            case SkillTargetType.SingleEnemy:
                return GetValidEtitiesByColumnLogic(enemyTeam);
            case SkillTargetType.Self:
                return new List<Entity>() { caster };
            case SkillTargetType.SingleAlly:
                return allyTeam;

            // Full Aoe
            case SkillTargetType.AllEnemies:
                return enemyTeam;
            case SkillTargetType.AllAllies:
                return allyTeam;

            default:
                return new List<Entity>();
        }

    }

    public List<Entity> GetTargets(Entity caster, SkillTargetType targetType, Entity selectedEntity, List<Entity> allEntities)
    {
        List<Entity> targets = new List<Entity>();

        switch (targetType)
        {
            //Single
            case SkillTargetType.Self:
                targets.Add(caster);
                break;
            case SkillTargetType.SingleEnemy:
                if (selectedEntity != null) targets.Add(selectedEntity);
                break;
            case SkillTargetType.SingleAlly:
                if (selectedEntity != null) targets.Add(selectedEntity);
                break;
            // Full Aoe
            case SkillTargetType.AllEnemies:
                targets = allEntities
                    .Where(e => e.Team != caster.Team && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                break;
            case SkillTargetType.AllAllies:
                targets = allEntities
                    .Where(e => e.Team == caster.Team && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                break;
        }


        return targets;
    }
}
