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
            case SkillTargetType.SingleEnemy:
                return GetValidEtitiesByColumnLogic(enemyTeam);
            case SkillTargetType.Selft:
                return new List<Entity>() { caster };

            default:
                return new List<Entity>();
        }

    }
}
