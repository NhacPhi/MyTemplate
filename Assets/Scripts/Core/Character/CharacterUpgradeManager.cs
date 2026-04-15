using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Chịu trách nhiệm duy nhất: xử lý toàn bộ nghiệp vụ NÂNG CẤP nhân vật.
/// Bao gồm: Thêm EXP / Lên Cấp (AddExp, AutoLevelUp),
///           Đột Phá Giới Hạn (Ascend),
///           Thăng Sao (StarUp).
/// 
/// Class này KHÔNG tự lưu data. Nó đọc và ghi thông qua CharacterProfileModel.
/// </summary>
public class CharacterUpgradeManager
{
    private readonly CharacterProfileModel _profile;
    private readonly GameDataBase _gameDataBase;
    private readonly InventoryManager _inventory;
    private readonly CurrencyManager _currency;

    public CharacterUpgradeManager(
        CharacterProfileModel profile,
        GameDataBase gameDataBase,
        InventoryManager inventory,
        CurrencyManager currency)
    {
        _profile      = profile;
        _gameDataBase = gameDataBase;
        _inventory    = inventory;
        _currency     = currency;
    }

    // ─────────────────────────────────────────────────────────────
    // EXP & LEVEL UP
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Trả về lượng EXP còn thiếu để lên cấp tiếp theo.
    /// </summary>
    public int GetExpRequiredForNextLevel()
    {
        var saveData = _profile.SaveData;
        var charConfig = _gameDataBase.GetCharacterConfig(saveData.ID);
        var expTier = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expNeedToUpdateLevel = _gameDataBase.GetExpConfig(expTier).UpExp[(saveData.Level + 1).ToString()];
        return expNeedToUpdateLevel - saveData.Exp;
    }

    /// <summary>
    /// Trả về tổng EXP cần thiết để đạt đến <paramref name="targetLevel"/> từ cấp hiện tại.
    /// </summary>
    public int GetExpNeededForTargetLevel(int targetLevel)
    {
        var saveData = _profile.SaveData;
        if (targetLevel <= saveData.Level || targetLevel > Definition.MAX_CHARACTER_LEVEL)
            return 0;

        var charConfig = _gameDataBase.GetCharacterConfig(saveData.ID);
        var expTier    = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig  = _gameDataBase.GetExpConfig(expTier);

        int totalNeeded = 0;
        for (int i = saveData.Level; i < targetLevel; i++)
        {
            string nextLevelStr = (i + 1).ToString();
            if (expConfig.UpExp.TryGetValue(nextLevelStr, out int reqExp))
                totalNeeded += reqExp;
        }

        return totalNeeded - saveData.Exp;
    }

    /// <summary>
    /// Trả về giới hạn level hiện tại (ví dụ 20, 40... nếu chưa đột phá, hoặc 100 nếu đã hết mốc).
    /// </summary>
    private int GetMaxLevelCap()
    {
        GetNextAscensionRequirements(out _, out int reqLevel, out _, out _);

        return reqLevel;
    }

    /// <summary>
    /// Thêm EXP cho nhân vật. Tự động xử lý lên cấp liên tiếp và trừ Coin.
    /// </summary>
    public void AddExp(int amount)
    {
        var saveData = _profile.SaveData;

        int levelCap = GetMaxLevelCap();
        if (amount < 0 || saveData.Level >= levelCap) return;

        saveData.Exp += amount;
        bool levelChanged = false;

        var charConfig = _gameDataBase.GetCharacterConfig(saveData.ID);
        var expTier    = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig  = _gameDataBase.GetExpConfig(expTier);

        while (saveData.Level < levelCap)
        {
            var nextLevelStr = (saveData.Level + 1).ToString();

            if (!expConfig.UpExp.TryGetValue(nextLevelStr, out int expNeedToUpdateLevel))
            {
                saveData.Level = Definition.MAX_CHARACTER_LEVEL;
                saveData.Exp   = 0;
                levelChanged   = true;
                break;
            }

            if (saveData.Exp >= expNeedToUpdateLevel)
            {
                int coinNeeded = Utility.GetCoinNeedToUpgradeCacultivate(saveData.Level + 1)
                               - Utility.GetCoinNeedToUpgradeCacultivate(saveData.Level);

                if (_currency.GetQuantityCurrecy(CurrencyType.Coin) >= coinNeeded)
                {
                    _currency.Spend(CurrencyType.Coin, coinNeeded);
                    saveData.Exp -= expNeedToUpdateLevel;
                    saveData.Level++;
                    levelChanged = true;
                }
                else
                {
                    break; // Ngưng lên cấp nếu thiếu Coin
                }
            }
            else
            {
                break;
            }
        }

        if (saveData.Level >= Definition.MAX_CHARACTER_LEVEL)
            saveData.Exp = 0;
    }

    /// <summary>
    /// Tiêu thụ một loại EXP item trong inventory rồi cộng EXP.
    /// </summary>
    public bool UseExpItem(string itemID, int quantity)
    {
        var saveData = _profile.SaveData;
        if (quantity <= 0 || saveData.Level >= GetMaxLevelCap()) return false;

        var itemConfig = _gameDataBase.GetItemConfig(itemID);
        if (itemConfig == null || itemConfig.Type != ItemType.Exp || itemConfig.Exp == null) return false;

        bool success = _inventory.ConsumeStackableItem(itemID, quantity);
        if (!success) return false;

        AddExp(itemConfig.Exp.Value * quantity);
        return true;
    }

    /// <summary>
    /// Tính tổng EXP có trong inventory (tất cả item loại Exp).
    /// </summary>
    public int GetTotalExpInInventory()
    {
        int totalExp = 0;
        foreach (var item in _inventory.Items)
        {
            var config = _gameDataBase.GetItemConfig(item.ID);
            if (config != null && config.Type == ItemType.Exp && config.Exp != null)
                totalExp += config.Exp.Value * item.Quantity;
        }
        return totalExp;
    }

    /// <summary>
    /// Tính cấp độ tối đa có thể đạt được với lượng EXP item hiện có trong inventory.
    /// </summary>
    public int GetMaxReachableLevelWithCurrentExpItemsAndCoin()
    {
        var saveData = _profile.SaveData;
        int totalExpAvailable = GetTotalExpInInventory() + saveData.Exp;
        int currentCoin = _currency.GetQuantityCurrecy(CurrencyType.Coin);
        int level = saveData.Level;

        int levelCap = GetMaxLevelCap();

        var charConfig = _gameDataBase.GetCharacterConfig(saveData.ID);
        var expTier    = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig  = _gameDataBase.GetExpConfig(expTier);

        while (level < levelCap)
        {
            string nextLevelStr = (level + 1).ToString();
            if (!expConfig.UpExp.TryGetValue(nextLevelStr, out int reqExp)) break;

            int coinNeeded = Utility.GetCoinNeedToUpgradeCacultivate(level + 1)
                           - Utility.GetCoinNeedToUpgradeCacultivate(level);

            if (totalExpAvailable >= reqExp && currentCoin >= coinNeeded)
            {
                totalExpAvailable -= reqExp;
                currentCoin -= coinNeeded;
                level++;
            }
            else break;
        }
        return level;
    }

    /// <summary>
    /// Tự động chọn tổ hợp EXP item tối ưu nhất để đủ <paramref name="expNeeded"/>.
    /// Trả về Dictionary[itemID → quantity].
    /// </summary>
    public Dictionary<string, int> AutoSelectExpItems(int expNeeded)
    {
        var selectedItems = new Dictionary<string, int>();
        if (expNeeded <= 0) return selectedItems;

        var expItems = new List<Tuple<string, int, int>>();
        foreach (var item in _inventory.Items)
        {
            var config = _gameDataBase.GetItemConfig(item.ID);
            if (config != null && config.Type == ItemType.Exp && config.Exp != null && item.Quantity > 0)
                expItems.Add(new Tuple<string, int, int>(item.ID, config.Exp.Value, item.Quantity));
        }

        // Ưu tiên dùng item có giá trị EXP lớn trước
        expItems.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        int currentExp = 0;
        foreach (var expItem in expItems)
        {
            string id              = expItem.Item1;
            int    expValue        = expItem.Item2;
            int    availableQty    = expItem.Item3;

            int amountNeeded = (expNeeded - currentExp) / expValue;
            int useAmount    = Math.Min(amountNeeded, availableQty);

            if (useAmount > 0)
            {
                selectedItems.Add(id, useAmount);
                currentExp += useAmount * expValue;
            }
        }

        // Nếu vẫn chưa đủ, thêm 1 item nhỏ nhất còn thừa
        if (currentExp < expNeeded)
        {
            expItems.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            foreach (var expItem in expItems)
            {
                string id           = expItem.Item1;
                int    expValue     = expItem.Item2;
                int    availableQty = expItem.Item3;

                int usedSoFar = selectedItems.ContainsKey(id) ? selectedItems[id] : 0;
                if (usedSoFar < availableQty)
                {
                    if (selectedItems.ContainsKey(id)) selectedItems[id]++;
                    else selectedItems.Add(id, 1);

                    currentExp += expValue;
                    break;
                }
            }
        }

        return selectedItems;
    }

    /// <summary>
    /// Tự động tiêu sách EXP và lên cấp đến <paramref name="targetLevel"/>.
    /// </summary>
    public bool AutoLevelUp(int targetLevel)
    {
        var saveData = _profile.SaveData;
        if (targetLevel <= saveData.Level) return false;

        int totalCoinsNeeded = Utility.GetCoinNeedToUpgradeCacultivate(targetLevel)
                             - Utility.GetCoinNeedToUpgradeCacultivate(saveData.Level);

        if (_currency.GetQuantityCurrecy(CurrencyType.Coin) < totalCoinsNeeded)
            return false;

        int neededExp = GetExpNeededForTargetLevel(targetLevel);

        // Đã đủ EXP sẵn → chỉ cần trigger vòng lặp lên cấp
        if (neededExp <= 0)
        {
            AddExp(0);
            return true;
        }

        var itemsToUse = AutoSelectExpItems(neededExp);

        int totalGrantedExp = 0;
        foreach (var kvp in itemsToUse)
        {
            bool success = _inventory.ConsumeStackableItem(kvp.Key, kvp.Value);
            if (success)
            {
                var config = _gameDataBase.GetItemConfig(kvp.Key);
                totalGrantedExp += config.Exp.Value * kvp.Value;
            }
        }

        if (totalGrantedExp > 0)
        {
            AddExp(totalGrantedExp);
            return true;
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────
    // ASCENSION (ĐỘT PHÁ GIỚI HẠN)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy thông tin yêu cầu cho lần đột phá tiếp theo.
    /// Trả về <c>false</c> nếu đã đạt cấp đột phá tối đa hoặc chưa đủ level.
    /// </summary>
    public bool GetNextAscensionRequirements(
        out int nextTier,
        out int requiredLevel,
        out List<CostIteam> requiredItems,
        out int requiredCoin)
    {
        var saveData  = _profile.SaveData;
        nextTier      = saveData.AscensionTier + 1;
        requiredLevel = 0;
        requiredItems = new List<CostIteam>();
        requiredCoin  = 0;

        var characterConfig  = _gameDataBase.GetCharacterConfig(saveData.ID);
        var ascensionConfig  = _gameDataBase.GetAscensionConfig(
            Utility.GetAscentionConfigIDByCharacterRare(characterConfig.Rare));

        if (ascensionConfig == null) return false;
        if (nextTier > ascensionConfig.MaxTier) return false; // Đã đạt tối đa

        if (ascensionConfig.TierConfigs.TryGetValue(nextTier.ToString(), out TierConfig tierConfig))
        {
            requiredLevel = tierConfig.LevelRequire;

            requiredItems = tierConfig.costs
                .Where(c => !c.ID.Equals("Coin", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var coinCost  = tierConfig.costs
                .FirstOrDefault(c => c.ID.Equals("Coin", StringComparison.OrdinalIgnoreCase));
            requiredCoin  = coinCost != null
                ? coinCost.Quantity
                : Utility.GetCoinNeedToAscendCharacter(nextTier);

            return saveData.Level >= requiredLevel;
        }

        return false;
    }

    /// <summary>
    /// Kiểm tra nhân vật có đủ điều kiện đột phá không (level + vật phẩm + coin).
    /// </summary>
    public bool IsEligibleForNextAscension()
    {
        return GetNextAscensionRequirements(
            out _, out int requiredLevel, out _, out _)
            && _profile.SaveData.Level >= requiredLevel;
    }

    /// <summary>
    /// Thực hiện đột phá: trừ Coin, trừ nguyên liệu, tăng AscensionTier.
    /// </summary>
    public bool Ascend()
    {
        if (!IsEligibleForNextAscension()) return false;

        if (!GetNextAscensionRequirements(
                out int nextTier, out _, out var requiredItems, out int requiredCoin))
            return false;

        if (_currency.GetQuantityCurrecy(CurrencyType.Coin) < requiredCoin) return false;

        foreach (var cost in requiredItems)
        {
            if (_inventory.GetItemQuantity(cost.ID) < cost.Quantity) return false;
        }

        _currency.Spend(CurrencyType.Coin, requiredCoin);

        foreach (var cost in requiredItems)
        {
            _inventory.ConsumeStackableItem(cost.ID, cost.Quantity);
        }

        _profile.SaveData.AscensionTier = nextTier;

        //_profile.OnLevelChanged?.Invoke();
        //_profile.OnStatsChanged?.Invoke();

        return true;
    }

    // ─────────────────────────────────────────────────────────────
    // STAR UP (THĂNG SAO)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy thông tin yêu cầu cho lần thăng sao tiếp theo.
    /// </summary>
    public bool GetNextStarUpRequirements(
        out int nextTier,
        out int requiredCoin,
        out int requiredQuantity)
    {
        var saveData       = _profile.SaveData;
        nextTier           = saveData.StarUp + 1;
        requiredCoin       = 0;
        requiredQuantity   = 0;

        var characterConfig = _gameDataBase.GetCharacterConfig(saveData.ID);
        var starUpConfig    = _gameDataBase.GetStarUpConfig(
            Utility.GetStarUpConfigIDByCharacterRare(characterConfig.Rare));

        if (starUpConfig == null) return false;
        if (nextTier > starUpConfig.MaxTier) return false;

        if (starUpConfig.Tiers.TryGetValue(nextTier.ToString(), out StarUpTierConfig tierConfig))
        {
            requiredCoin     = tierConfig.CostCoin;
            requiredQuantity = tierConfig.Quantity;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Kiểm tra nhân vật có đủ Coin và mảnh nhân vật để thăng sao không.
    /// </summary>
    public bool IsEligibleForNextStarUp()
    {
        if (GetNextStarUpRequirements(out _, out int requiredCoin, out int requiredQty))
        {
            return _currency.GetQuantityCurrecy(CurrencyType.Coin) >= requiredCoin
                && _inventory.GetItemQuantity(_profile.SaveData.ID)  >= requiredQty;
        }
        return false;
    }

    /// <summary>
    /// Thực hiện thăng sao: trừ Coin, tiêu mảnh nhân vật, tăng StarUp.
    /// </summary>
    public bool StarUp()
    {
        if (!IsEligibleForNextStarUp()) return false;

        if (!GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQty))
            return false;

        _currency.Spend(CurrencyType.Coin, requiredCoin);
        _inventory.ConsumeStackableItem(_profile.SaveData.ID, requiredQty);

        _profile.SaveData.StarUp = nextTier;

        //_profile.OnStatsChanged?.Invoke();

        return true;
    }
}
