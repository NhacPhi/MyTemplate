using System;

public class ItemData 
{
    private string id;
    private int quantity;
    private ItemType type;

    public string ID { get { return id; } set { id = value; } }
    public int Quantity { get { return quantity; } set { quantity = value; } }

    public ItemType Type { get { return type;} set { type = value; } }
}
