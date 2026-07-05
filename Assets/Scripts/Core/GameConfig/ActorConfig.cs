using System;
using Newtonsoft.Json;

[Serializable]
public class ActorConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("dialogue_default")]
    public string DialogueDefault;

    [JsonProperty("location_hash")]
    public long LocationName;

    [JsonIgnore]
    public ActorSO ActorSo;
}
