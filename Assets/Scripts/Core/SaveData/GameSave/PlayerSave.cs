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
}
