using System;
using Newtonsoft.Json;

public enum ObjectiveType
{
    KillEnemy,         // Tiêu diệt quái (TargetID = EnemyID, Amount = Số lượng)
    PickupItem,        // Thu thập vật phẩm (TargetID = ItemID, Amount = Số lượng)
    UpgradeCharacter,  // Cường hóa nhân vật (TargetID = Rỗng hoặc ID nhân vật, Amount = Số lần)
    UpgradeWeapon,     // Cường hóa vũ khí (TargetID = Rỗng, Amount = Số lần)
    WinBattle,         // Vượt ải (TargetID = StageID, Amount = Số lần)
    TalkToNPC,         // Tương tác NPC (TargetID = ActorID, Amount = 1)
    Purchase,          // Mua sắm tại shop
    Summon             // Gacha (TargetID = "Character" hoặc "Weapon", Amount = Số lượt)
}

[Serializable]
public class DailyQuestConfig
{
    [JsonProperty("id")] 
    public string ID;

    [JsonProperty("name_hash")] 
    public long Name;

    [JsonProperty("des_hash")] 
    public long Description;

    [JsonProperty("target_hash")] 
    public long TargetHash;

    [JsonProperty("location_hash")] 
    public long LocationHash;

    [JsonProperty("reward_id")] 
    public string RewardID;
    
    [JsonProperty("objective_type")] 
    public string ObjectiveTypeStr; // We will parse enum manually if needed

    [JsonProperty("target_id")] 
    public string TargetID; 

    [JsonProperty("require_amount")] 
    public int RequireAmount;

    [JsonIgnore]
    public ObjectiveType ObjectiveType
    {
        get
        {
            if (Enum.TryParse<ObjectiveType>(ObjectiveTypeStr, true, out var result))
            {
                return result;
            }
            return ObjectiveType.KillEnemy; // Default
        }
    }
}
