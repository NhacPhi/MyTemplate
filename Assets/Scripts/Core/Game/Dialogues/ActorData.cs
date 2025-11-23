using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorData
{
    private string id;
    private string name;
    private Sprite image;

    public string ID => id;
    public string Name => name;
    public Sprite Image => image;   
    public void InitData(string id, string name, Sprite image)
    {
        this.id = id;
        this.name = name;
        this.image = image;
    }
}
