using System;
using System.Collections.Generic;

public class RewardPayLoad
{
    private string questID;
    private List<ItemReward> rewardItems;

    public string QuestID { get { return questID; } set { questID = value; } }
    public List<ItemReward> RewardItems { get { return rewardItems; } set { rewardItems = value; } }
    public RewardPayLoad(string questId)
    {
        questID = questId;
        rewardItems = new List<ItemReward>();
    }
}
public class ItemReward
{
    private string itemID;
    private int count;

    public string ItemID { get { return itemID; } set { itemID = value; } }
    public int Count { get { return count; } set { count = value; } }

    public ItemReward(string ItemId, int Count)
    {
        itemID = ItemId;
        count = Count;
    }
}
