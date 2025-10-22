using System;

[Serializable]
public class AvatarConfig 
{
    private string id;

    private string rare;

    public string ID { get { return id; } set { id = value; } }
    public string Rare { get { return rare; } set { rare = value; } }
}
