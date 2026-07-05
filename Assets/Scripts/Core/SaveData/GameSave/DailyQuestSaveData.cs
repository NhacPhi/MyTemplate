using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class DailyQuestSaveData
{
    [JsonProperty("last_reset_time_ticks")]
    public long LastResetTimeTicks;

    [JsonProperty("active_daily_quests")]
    public Dictionary<string, int> ActiveDailyQuests = new Dictionary<string, int>();

    [JsonProperty("completed_daily_quests")]
    public List<string> CompletedDailyQuests = new List<string>();

    [JsonProperty("tracked_quest_id")]
    public string TrackedQuestID;
}
