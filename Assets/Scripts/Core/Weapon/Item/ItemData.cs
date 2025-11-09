using System;

public class ItemData 
{
    private string id;
    private int quanlity;
    private ItemType type;

    public string ID { get { return id; } set { id = value; } }
    public int Quanlity { get { return quanlity; } set { quanlity = value; } }

    public ItemType Type { get { return type;} set { type = value; } }
}
