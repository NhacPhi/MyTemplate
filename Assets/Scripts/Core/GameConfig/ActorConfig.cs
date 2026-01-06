using System;
using Newtonsoft.Json;

[Serializable]
public class ActorConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("dialogue_default")]
    public string DialogueDefault;

    [JsonIgnore]
    public ActorSO ActorSo;
}
