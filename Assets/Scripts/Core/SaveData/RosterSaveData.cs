using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosterSaveData
{
    [JsonProperty("characters")]
    public List<CharacterSaveData> Characters;

    [JsonProperty("active_slots")]
    public List<ActiveSlotData> ActiveSlots;

    [JsonProperty("active_global_buffs")]
    public List<ActiveGlobalBuff> ActiveGlobalBuffs = new List<ActiveGlobalBuff>();

    public CharacterSaveData GetCharacter(string id)
    {
        return Characters.Find(v => v.ID == id);
    }

    public CharacterSaveData GetIDOfFirstCharacter()
    {
        if (Characters == null || Characters.Count == 0)
        {
            return null;
        }

        return Characters[0];
    }
}

[System.Serializable]
public class ActiveGlobalBuff
{
    [JsonProperty("stat_type")] public StatType StatType;
    [JsonProperty("mod_type")] public ModifyType ModifierType;
    [JsonProperty("value")] public float Value;
    [JsonProperty("expiration_time_ticks")] public long ExpirationTimeTicks;

    [JsonIgnore]
    public bool IsActive => System.DateTime.UtcNow.Ticks < ExpirationTimeTicks;
}
