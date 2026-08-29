using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RosterSaveData
{
    [JsonProperty("characters")]
    public List<CharacterSaveData> Characters = new List<CharacterSaveData>();

    [JsonProperty("active_slots")]
    public List<ActiveSlotData> ActiveSlots = new List<ActiveSlotData>();

    [JsonProperty("active_global_buffs")]
    public List<ActiveGlobalBuff> ActiveGlobalBuffs = new List<ActiveGlobalBuff>();

    public CharacterSaveData GetCharacter(string id)
    {
        if (Characters == null) return null;
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

    public bool AddCharacter(string id)
    {
        if (Characters == null) Characters = new List<CharacterSaveData>();
        if (GetCharacter(id) != null) return false;

        var newChar = new CharacterSaveData
        {
            ID = id,
            Level = 1,
            Exp = 0,
            AscensionTier = 0,
            StarUp = 0,
            Weapon = "",
            Armors = new List<PartSaveData>()
        };
        Characters.Add(newChar);
        return true;
    }
}

[Serializable]
public class ActiveGlobalBuff
{
    [JsonProperty("stat_type")] public StatType StatType;
    [JsonProperty("mod_type")] public ModifyType ModifierType;
    [JsonProperty("value")] public float Value;
    [JsonProperty("expiration_time_ticks")] public long ExpirationTimeTicks;

    [JsonIgnore]
    public bool IsActive => System.DateTime.UtcNow.Ticks < ExpirationTimeTicks;
}
