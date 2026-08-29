using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSave
{
    [JsonProperty("account_info")]
    public AccountSaveData Account { get; set; } = new AccountSaveData();

    [JsonProperty("inventory")]
    public InventorySaveData Inventory { get; set; } = new InventorySaveData();

    [JsonProperty("roster")]
    public RosterSaveData Roster { get; set; } = new RosterSaveData();

    [JsonProperty("shop")]
    public ShopSaveData Shop { get; set; } = new ShopSaveData();

    [JsonProperty("gacha")]
    public GachaSaveData Gacha { get; set; } = new GachaSaveData();

    [JsonProperty("world_state")]
    public WorldSaveData WorldState { get; set; } = new WorldSaveData();

    [JsonProperty("quest")]
    public QuestSaveData Quest { get; set; } = new QuestSaveData();

    [JsonProperty("daily_quest")]
    public DailyQuestSaveData DailyQuest { get; set; } = new DailyQuestSaveData();

    [JsonProperty("seven_day_login")]
    public SevenDayLoginSaveData SevenDayLogin { get; set; } = new SevenDayLoginSaveData();
}
