using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BattleConfig
{
    [JsonProperty("nam_hash")]
    public long Name;

    [JsonProperty("background")]
    public string BackGround;

    [JsonProperty("reward")]
    public string Reward;

    [JsonProperty("enemies")]
    public List<StageEnemyCompoment> Enemies;
}

[Serializable]
public class StageEnemyCompoment
{
    [JsonProperty("slot")]
    public int Slot;

    [JsonProperty("enemy_id")]
    public string EnemyID;

    [JsonProperty("enemy_level")]
    public int EnemyLevel;
}
