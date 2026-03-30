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
