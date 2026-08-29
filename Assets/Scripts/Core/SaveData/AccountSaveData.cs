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
    public int Level = 1;

    [JsonProperty("current_exp")]
    public int CurrentExp = 0;

    [JsonProperty("avatar_icon")]
    public string AvatarIcon;

    [JsonProperty("claimed_redeem_codes")]
    public List<string> ClaimedRedeemCodes = new List<string>();

    public void SetAvatarIcon(string id)
    {
        AvatarIcon = id;
    }

    public bool HasClaimedRedeemCode(string code)
    {
        if (ClaimedRedeemCodes == null) ClaimedRedeemCodes = new List<string>();
        return ClaimedRedeemCodes.Contains(code);
    }

    public void AddClaimedRedeemCode(string code)
    {
        if (ClaimedRedeemCodes == null) ClaimedRedeemCodes = new List<string>();
        if (!ClaimedRedeemCodes.Contains(code))
        {
            ClaimedRedeemCodes.Add(code);
        }
    }
}
