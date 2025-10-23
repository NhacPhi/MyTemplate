using System;

public class Item 
{
    private string id;
    private int count;
    private ItemType type;

    public string ID { get { return id; } set { id = value; } }
    public int Count { get { return count; } set { count = value; } }

    public ItemType Type { get { return type;} set { type = value; } }
}
