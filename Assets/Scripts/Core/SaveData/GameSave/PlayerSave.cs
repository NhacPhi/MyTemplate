using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


[Serializable]
public class PlayerSave
{
    [JsonProperty("account_info")]
    public AccountSaveData Account { get; private set; }

    [JsonProperty("inventory")]
    public InventorySaveData Inventory { get; private set; }

    [JsonProperty("roster")]
    public RosterSaveData Roster { get; private set; }

    [JsonProperty("shop")]
    public ShopSaveData Shop { get; private set; } = new ShopSaveData();

    [JsonProperty("gacha")]
    public GachaSaveData Gacha { get; private set; } = new GachaSaveData();

    [JsonProperty("world_state")]
    public WorldSaveData WorldState { get; private set; } = new WorldSaveData();

    [JsonProperty("quest")]
    public QuestSaveData Quest { get; private set; } = new QuestSaveData();

    [JsonProperty("daily_quest")]
    public DailyQuestSaveData DailyQuest { get; private set; } = new DailyQuestSaveData();
}
