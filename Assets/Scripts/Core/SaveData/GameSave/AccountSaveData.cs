using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public class AccountSaveData
{
    [JsonProperty("player_name")]
    public string PlayerName;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("avatar_icon")]
    public string AvatarIcon;

    public void SetAvatarIcon(string id)
    {
        AvatarIcon = id;
    }
}
