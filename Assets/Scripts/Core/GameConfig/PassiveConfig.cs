using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class PassiveConfig 
{
    [JsonProperty("desc_template_hash")]
    public long DescTemplate;

    [JsonProperty("param0_values")]
    public List<float> FirstParams;

    [JsonProperty("param1_values")]
    public List<float> SecondParams;
}
