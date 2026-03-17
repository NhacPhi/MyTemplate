using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnOrderSystem 
{
    private List<Entity> _entities;

    private const float MAX_AP = 1000f;

    public void Inititalize(List<Entity> entities)
    {
        _entities = entities;

        foreach (var entity in _entities)
        {
            var stats = entity.GetComponent<EntityStats>(); 

            stats.CurrentAV = MAX_AP / stats.GetStat(StatType.SPEED).Value;
        }
    }

    public Entity GetNextCharacter()
    {
        var nextEntity = _entities
            .Where(e => !e.GetComponent<EntityStats>().IsDead)
            .OrderBy(e => e.GetComponent<EntityStats>().CurrentAV)
            .First();

        var nextStats = nextEntity.GetComponent<EntityStats>();
        float minAV = nextStats.CurrentAV;

        foreach(var entity in _entities)
        {
            var stats = entity.GetComponent<EntityStats>();
            if (stats.IsDead) continue;

            stats.CurrentAV -= minAV;

            if(stats.CurrentAV < 0) stats.CurrentAV = 0;
        }

        return nextEntity;
    }

    public void ResetEntityAV(Entity entity)
    {
        var stats = entity.GetComponent<EntityStats>();
        stats.CurrentAV = MAX_AP / stats.GetStat(StatType.SPEED).Value;
    }
}
