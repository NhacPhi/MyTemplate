using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        var aliveEntities = _entities
            .Where(e => e != null && e.GetComponent<EntityStats>() != null && !e.GetComponent<EntityStats>().IsDead)
            .OrderBy(e => e.GetComponent<EntityStats>().CurrentAV)
            .ToList();

        if (aliveEntities.Count == 0) return null;

        var nextEntity = aliveEntities[0];
        var nextStats = nextEntity.GetComponent<EntityStats>();
        float minAV = nextStats.CurrentAV;

        foreach(var entity in _entities)
        {
            if (entity == null) continue;
            var stats = entity.GetComponent<EntityStats>();
            if (stats == null || stats.IsDead) continue;

            stats.CurrentAV -= minAV;

            if(stats.CurrentAV < 0) stats.CurrentAV = 0;
        }

        return nextEntity;
    }

    public void ResetEntityAV(Entity entity)
    {
        if (entity == null) return;
        var stats = entity.GetComponent<EntityStats>();
        if (stats == null) return;
        var speedStat = stats.GetStat(StatType.SPEED);
        float speed = (speedStat != null && speedStat.Value > 0) ? speedStat.Value : 100f;
        float baseAV = MAX_AP / speed;

        // Nếu trong lượt vừa rồi entity được nhận AdvanceAction (CurrentAV < 0), ta trừ phần được kéo vào BaseAV của vòng mới
        if (stats.CurrentAV < 0)
        {
            stats.CurrentAV = Mathf.Max(0, baseAV + stats.CurrentAV);
        }
        else
        {
            stats.CurrentAV = baseAV;
        }
    }

    public void AdvanceAction(Entity entity, float percentAdvance)
    {
        if (entity == null) return;
        var stats = entity.GetComponent<EntityStats>();
        if (stats == null || stats.IsDead) return;

        var speedStat = stats.GetStat(StatType.SPEED);
        float speed = (speedStat != null && speedStat.Value > 0) ? speedStat.Value : 100f;
        float baseAV = MAX_AP / speed;

        // Giảm CurrentAV tương ứng với số % BaseAV được kéo
        stats.CurrentAV -= baseAV * (percentAdvance / 100f);
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
