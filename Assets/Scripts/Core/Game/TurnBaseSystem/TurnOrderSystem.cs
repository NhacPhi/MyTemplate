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

    public List<Entity> PredictTurnOrder(int turnsToPredict = 6)
    {
        List<Entity> predictedOrder = new List<Entity>();

        Dictionary<Entity, float> simulatedAVs = new Dictionary<Entity, float>();

        foreach(var entity in _entities)
        {
            var stats = entity.GetComponent<EntityStats>();

            if(!stats.IsDead)
            {
                simulatedAVs[entity] = stats.CurrentAV;
            }
        }

        // all entites dead
        if(simulatedAVs.Count == 0) return predictedOrder;

        for(int i = 0; i < turnsToPredict; i++)
        {
            var nextEntityEntry = simulatedAVs.OrderBy(kvp => kvp.Value).First();
            Entity nextEntity = nextEntityEntry.Key;
            float minAV = nextEntityEntry.Value;

            predictedOrder.Add(nextEntity);

            List<Entity> activeEntities = simulatedAVs.Keys.ToList();
            foreach (var entity in activeEntities)
            {
                simulatedAVs[entity] -= minAV;
                if (simulatedAVs[entity] < 0) simulatedAVs[entity] = 0;
            }

            var stats = nextEntity.GetComponent<EntityStats>();
            float speed = stats.GetStat(StatType.SPEED).Value;
            simulatedAVs[nextEntity] = MAX_AP / speed;
        }

        return predictedOrder;
    }
}
