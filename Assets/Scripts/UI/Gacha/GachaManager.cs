using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class GachaManager
{
    private readonly GameDataBase _db;
    private readonly GachaRuntimeManager _runtimeManager;
    private readonly InventoryManager _inventoryManager;
    private readonly SaveSystem _saveSystem;

    [Inject]
    public GachaManager(GameDataBase db, GachaRuntimeManager runtimeManager, InventoryManager inventoryManager, SaveSystem saveSystem)
    {
        _db = db;
        _runtimeManager = runtimeManager;
        _inventoryManager = inventoryManager;
        _saveSystem = saveSystem;
    }

    public List<GachaItemResult> RollGacha(string bannerId, int count)
    {
        var config = _db.GetGachaConfig(bannerId);
        if (config == null)
        {
            Debug.LogError($"[GachaManager] Cannot find banner config for {bannerId}");
            return new List<GachaItemResult>();
        }

        var results = new List<GachaItemResult>();
        for (int i = 0; i < count; i++)
        {
            results.Add(RollSingle(config));
        }

        return results;
    }

    private GachaItemResult RollSingle(GachaConfig config)
    {
        var runtimeData = _runtimeManager.GetRuntimeData(config.BannerId);
        
        runtimeData.TotalSummonCount++;
        runtimeData.PityCount++;

        // 1. Xác định Rarity (Độ hiếm)
        int rolledRarity = DetermineRarity(config, runtimeData);

        // Reset pity nếu ra đồ 5 sao (Legendary)
        if (rolledRarity >= 5) 
        {
            runtimeData.PityCount = 0;
        }

        // 2. Lấy vật phẩm trong Pool tương ứng với Rarity
        var item = PickItemFromPool(config, runtimeData, rolledRarity);

        // 3. Lưu dữ liệu Pity và Guarantee xuống ổ cứng
        _runtimeManager.SaveData();

        return item;
    }

    private int DetermineRarity(GachaConfig config, BannerRuntimeData runtimeData)
    {
        // Kiểm tra Pity (Bảo hiểm cứng - Hard Pity)
        if (runtimeData.PityCount >= config.PityLimit)
        {
            return 5; // Ép ra 5 sao nếu chạm pity
        }

        // Tung xúc xắc 0.0 -> 1.0
        float roll = Random.value;
        float currentProb = 0f;

        // Ưu tiên duyệt từ Rarity cao xuống thấp (5 -> 4 -> 3)
        var sortedRates = config.Rates.OrderByDescending(r => int.Parse(r.Key));

        foreach (var kvp in sortedRates)
        {
            float rate = kvp.Value.BaseRate;
            
            currentProb += rate;
            if (roll <= currentProb)
            {
                return int.Parse(kvp.Key);
            }
        }

        // Mặc định trả về độ hiếm thấp nhất nếu xúc xắc nằm ngoài khoảng (rất hiếm khi xảy ra nếu tổng rate = 1)
        return int.Parse(sortedRates.Last().Key);
    }

    private GachaItemResult PickItemFromPool(GachaConfig config, BannerRuntimeData runtimeData, int rarity)
    {
        var poolForRarity = config.Pool.Where(p => p.Rarity == rarity).ToList();

        // Xử lý 50/50 cho thẻ 5 sao (Nếu config.Rates quy định IsGuarantee)
        if (config.Rates.TryGetValue(rarity.ToString(), out var rateConfig) && rateConfig.IsGuarantee)
        {
            // Lọc danh sách Rate Up hoặc Target người chơi đã chọn
            var rateUpPool = poolForRarity.Where(p => 
                p.IsRateUp || 
                (config.AllowSelection && p.ItemId == runtimeData.SelectedTargetId)
            ).ToList();

            var standardPool = poolForRarity.Where(p => !rateUpPool.Contains(p)).ToList();

            if (rateUpPool.Count > 0)
            {
                if (runtimeData.IsNextSSRGuaranteed)
                {
                    // Lần trước thua 50/50 -> Lần này chắc chắn trúng Rate Up
                    poolForRarity = rateUpPool;
                    runtimeData.IsNextSSRGuaranteed = false; 
                }
                else
                {
                    // Lần trước đã trúng hoặc mới bắt đầu -> Tung 50/50
                    if (Random.value <= 0.5f)
                    {
                        // Thắng 50/50 (Nổ vàng Rate Up)
                        poolForRarity = rateUpPool;
                        runtimeData.IsNextSSRGuaranteed = false;
                    }
                    else
                    {
                        // Thua 50/50 (Nổ vàng Lệch rate)
                        poolForRarity = standardPool.Count > 0 ? standardPool : poolForRarity;
                        runtimeData.IsNextSSRGuaranteed = true; // Bật bảo hiểm cho lần 5 sao kế tiếp
                    }
                }
            }
        }

        // Thuật toán Weighted Random: Chọn thẻ ngẫu nhiên dựa vào trọng số (Weight)
        int totalWeight = poolForRarity.Sum(p => p.Weight);
        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        GachaPoolItem selectedItem = poolForRarity.FirstOrDefault();

        foreach (var p in poolForRarity)
        {
            currentWeight += p.Weight;
            if (randomWeight < currentWeight)
            {
                selectedItem = p;
                break;
            }
        }

        string finalItemId = selectedItem != null ? selectedItem.ItemId : "unknown";

        bool isChar = false;
        var itemConfig = _db.GetItemConfig(finalItemId);
        if (itemConfig != null)
        {
            isChar = itemConfig.Type == ItemType.Avatar || itemConfig.Type == ItemType.Shard;
        }
        else
        {
            var charConfig = _db.GetCharacterConfig(finalItemId);
            isChar = charConfig != null;
        }

        // Tạo kết quả
        var result = new GachaItemResult
        {
            itemId = finalItemId,
            rarity = (Rare)rarity, // Ép kiểu 5 -> Legendary, 4 -> Epic, v.v..
            isCharacter = isChar
        };

        ProcessGachaItemToInventory(result);

        return result;
    }

    private void ProcessGachaItemToInventory(GachaItemResult item)
    {
        if (item.isCharacter)
        {
            var existingChar = _saveSystem.Player.Roster.GetCharacter(item.itemId);
            if (existingChar != null)
            {
                // Đã sở hữu -> Quy đổi thành mảnh
                string shardId = item.itemId; // Sử dụng chung ID tướng làm ID mảnh
                int shardAmount = 0;
                switch (item.rarity)
                {
                    case Rare.Uncommon: shardAmount = 10; break; // Rarity 2
                    case Rare.Rare: shardAmount = 20; break;     // Rarity 3
                    case Rare.Epic: shardAmount = 30; break;     // Rarity 4
                    case Rare.Legendary: shardAmount = 60; break;// Rarity 5
                    default: shardAmount = 10; break;
                }

                _inventoryManager.AddStackableItem(shardId, ItemType.Shard, shardAmount);
                item.isConverted = true;
                item.convertedShardAmount = shardAmount;
            }
            else
            {
                // Chưa sở hữu -> Thêm vào Roster
                var newChar = new CharacterSaveData
                {
                    ID = item.itemId,
                    Level = 1,
                    Exp = 0,
                    AscensionTier = 0,
                    StarUp = 0,
                    Weapon = "",
                    Armors = new List<PartSaveData>()
                };
                _saveSystem.Player.Roster.Characters.Add(newChar);

                // Kích hoạt event cập nhật UI
                UIEvent.OnCharacterAdded?.Invoke(item.itemId);
            }
        }
        else
        {
            // Xử lý vũ khí
            var itemConfig = _db.GetItemConfig(item.itemId);
            if (itemConfig != null && itemConfig.Type == ItemType.Weapon)
            {
                var newWeapon = new WeaponSaveData
                {
                    UUID = System.Guid.NewGuid().ToString(),
                    TemplateID = item.itemId,
                    CurrentLevel = 1,
                    CurrentUpgrade = 0,
                    Equip = ""
                };
                _inventoryManager.AddWeapon(newWeapon);
            }
            else
            {
                // Các item khác (nếu có)
                _inventoryManager.AddStackableItem(item.itemId, itemConfig != null ? itemConfig.Type : ItemType.Item, 1);
            }
        }
    }
}
