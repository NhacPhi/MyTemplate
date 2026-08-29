using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class SevenDayLoginSaveData
{
    [JsonProperty("selected_day7_character_id")]
    public string SelectedDay7CharacterId = "";

    [JsonProperty("claimed_days")]
    public List<int> ClaimedDays = new List<int>();

    [JsonProperty("last_login_date")]
    public string LastLoginDate = "";

    [JsonProperty("current_login_day")]
    public int CurrentLoginDay = 1;
}
