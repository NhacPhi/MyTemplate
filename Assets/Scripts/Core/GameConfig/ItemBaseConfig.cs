using System;
using UnityEngine;

public class ItemBaseConfig
{
    private string id;
    private string name;
    private string description;
    private Rare rare;

    public string ID { get { return id; } set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Description { get { return description; } set { description = value; } }
    public Rare Rare { get { return rare; } set { rare = value; } }
}
