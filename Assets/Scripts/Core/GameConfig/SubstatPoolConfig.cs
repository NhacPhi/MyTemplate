using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public class SubstatPoolConfig
{
    [JsonProperty("Pools")]
    public List<SubstatCompoment> Pools;
}
