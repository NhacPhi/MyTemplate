using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BattleConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("background")]
    public string BackGround;

    [JsonProperty("reward")]
    public string Reward;

    [JsonProperty("exp_reward")]
    public int ExpReward;

    [JsonProperty("enemies")]
    public List<StageEnemyComponent> Enemies;

    [JsonIgnore]
    public Sprite Sprite { get; set; }
}

[Serializable]
public class StageEnemyComponent
{
    [JsonProperty("slot")]
    public int Slot;

    [JsonProperty("enemy_id")]
    public string EnemyID;

    [JsonProperty("enemy_level")]
    public int EnemyLevel;

    [JsonProperty("boss")]
    public bool IsBoss;
}
