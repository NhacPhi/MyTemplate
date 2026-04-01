using System;
using System.Collections.Generic;
using Newtonsoft.Json;
public class ExpConfig 
{
    [JsonProperty("total_exp")]
    public Dictionary<string, int> TotalExps;

    [JsonProperty("up_exp")]
    public Dictionary<string, int> UpExp;
}
