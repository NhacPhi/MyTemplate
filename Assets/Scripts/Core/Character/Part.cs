using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part
{
    private string id;
    private ArmorPart type;

    public string ID { get { return id; } set { id = value; } }
    public ArmorPart Type { get { return type; } set { type = value; } }
}
