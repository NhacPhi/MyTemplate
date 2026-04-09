using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using System;
using System.Linq;

public class CharacterProfileModel : IStatProvider
{
    private CharacterSaveData _saveData;

    public CharacterSaveData SaveData => _saveData;
    private CharacterConfig _baseConfig;
    public CharacterConfig BaseConfig => _baseConfig;

    private GameDataBase _gameDataBaae;
    private InventoryManager _inventory;
    private CurrencyManager _currency;

    public EquipmentManager Equipment { get; private set; } = new EquipmentManager();

    public event Action OnLevelChanged;
    public event Action OnEquipmentChanged;
    public event Action OnSkilUpgraded;
    public event Action OnStatsChanged;
    private SetBonusEvaluator _setBonusEvaluator;
    public void Init(CharacterSaveData saveData,  CharacterConfig baseConfig, GameDataBase gameDataBase, InventoryManager inventory, CurrencyManager currency)
    {
        _saveData = saveData;
        _baseConfig = baseConfig;
        _gameDataBaae = gameDataBase;
        _inventory = inventory;
        _currency = currency;

        _setBonusEvaluator = new SetBonusEvaluator(_gameDataBaae);
        Equipment.Init(_setBonusEvaluator);

        LoadEquipmentsFromSave();
    }

    private void LoadEquipmentsFromSave()
    {
        // check weapon equip
        if(!string.IsNullOrEmpty(SaveData.Weapon))
        {
            var weaponSave = _inventory.GetWeapon(SaveData.Weapon);
            if(weaponSave != null)
            { 
                var weaponConfig = _gameDataBaae.GetItemConfig(weaponSave.TemplateID);
                var weaponData = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig);
                Equipment.Equip(weaponData);
            }
        }

        // check armor equip
        if(SaveData.Armors != null)
        {
            foreach(var armorPart in SaveData.Armors)
            {
                var armorSave = _inventory.GetArmor(armorPart.ID);
                if(armorSave != null)
                {
                    var armorConfig = _gameDataBaae.GetItemConfig(armorSave.TemplateID);
                    var armorData = EquipmentFactory.CreateArmorData(armorSave, armorConfig);
                    Equipment.Equip(armorData);
                }
            }
        }
    }

    public int GetExpRequiredForNextLevel()
    {
        var currentExp = SaveData.Exp;
        var charConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);

        var expTier = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);

        var expNeedToUpdateLevel = _gameDataBaae.GetExpConfig(expTier).UpExp[(SaveData.Level + 1).ToString()];

        return expNeedToUpdateLevel - currentExp;
    }

    public bool LevelUpNextLevel()
    {
        if (SaveData.Level >= Definition.CharacterMaxLevel) return false;
        int expNeeded = GetExpRequiredForNextLevel();

        return true;
    }

    public void AddExp(int amount)
    {
        if (amount <= 0 || SaveData.Level >= Definition.CharacterMaxLevel) return;

        SaveData.Exp += amount;
        bool levelChanged = false;

        var charConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);
        var expTier = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig = _gameDataBaae.GetExpConfig(expTier);

        while (SaveData.Level < Definition.CharacterMaxLevel)
        {
            var nextLevelStr = (SaveData.Level + 1).ToString();
            
            // Check if configure exists for next level
            if (!expConfig.UpExp.TryGetValue(nextLevelStr, out int expNeedToUpdateLevel))
            {
                SaveData.Level = Definition.CharacterMaxLevel;
                SaveData.Exp = 0;
                levelChanged = true;
                break;
            }

            if (SaveData.Exp >= expNeedToUpdateLevel)
            {
                int coinNeeded = Utility.GetCoinNeedToUpgradeCacultivate(SaveData.Level + 1) - Utility.GetCoinNeedToUpgradeCacultivate(SaveData.Level);
                
                if (_currency.GetQuantityCurrecy(CurrencyType.Coin) >= coinNeeded)
                {
                    _currency.Spend(CurrencyType.Coin, coinNeeded);
                    SaveData.Exp -= expNeedToUpdateLevel;
                    SaveData.Level++;
                    levelChanged = true;
                }
                else
                {
                    break; // Ngưng lên cấp nếu thiếu coin
                }
            }
            else
            {
                break;
            }
        }

        if (SaveData.Level >= Definition.CharacterMaxLevel)
        {
            SaveData.Exp = 0;
        }

        if (levelChanged)
        {
            OnLevelChanged?.Invoke();
            OnStatsChanged?.Invoke();
        }
    }

    public bool UseExpItem(string itemID, int quantity)
    {
        if (quantity <= 0 || SaveData.Level >= Definition.CharacterMaxLevel) return false;

        var itemConfig = _gameDataBaae.GetItemConfig(itemID);
        if (itemConfig == null || itemConfig.Type != ItemType.Exp || itemConfig.Exp == null) return false;

        bool success = _inventory.ConsumeStackableItem(itemID, quantity);
        if (!success) return false;

        int totalExp = itemConfig.Exp.Value * quantity;
        AddExp(totalExp);
        return true;
    }

    // -------------------------------------------------------------
    // LOGIC CHO UI TỰ ĐỘNG THÊM SÁCH (BUTTON 1 VÀ BUTTON 2)
    // -------------------------------------------------------------

    public int GetExpNeededForTargetLevel(int targetLevel)
    {
        if (targetLevel <= SaveData.Level || targetLevel > Definition.CharacterMaxLevel) 
            return 0;
            
        int totalNeeded = 0;
        var charConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);
        var expTier = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig = _gameDataBaae.GetExpConfig(expTier);

        for (int i = SaveData.Level; i < targetLevel; i++)
        {
            string nextLevelStr = (i + 1).ToString();
            if (expConfig.UpExp.TryGetValue(nextLevelStr, out int reqExp))
            {
                totalNeeded += reqExp;
            }
        }

        return totalNeeded - SaveData.Exp;
    }

    public int GetTotalExpInInventory()
    {
        int totalExp = 0;
        foreach (var item in _inventory.Items)
        {
            var config = _gameDataBaae.GetItemConfig(item.ID);
            if (config != null && config.Type == ItemType.Exp && config.Exp != null)
            {
                totalExp += config.Exp.Value * item.Quantity;
            }
        }
        return totalExp;
    }

    public int GetMaxReachableLevelWithCurrentExpItems()
    {
        int totalExpAvailable = GetTotalExpInInventory() + SaveData.Exp;
        int level = SaveData.Level;
        
        var charConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);
        var expTier = Utility.GetExpConfigIDByCharacterRare(charConfig.Rare);
        var expConfig = _gameDataBaae.GetExpConfig(expTier);

        while (level < Definition.CharacterMaxLevel)
        {
            string nextLevelStr = (level + 1).ToString();
            if (!expConfig.UpExp.TryGetValue(nextLevelStr, out int reqExp))
                break;

            if (totalExpAvailable >= reqExp)
            {
                totalExpAvailable -= reqExp;
                level++;
            }
            else
            {
                break;
            }
        }
        return level;
    }

    public Dictionary<string, int> AutoSelectExpItems(int expNeeded)
    {
        var selectedItems = new Dictionary<string, int>();
        if (expNeeded <= 0) return selectedItems;

        var expItems = new List<Tuple<string, int, int>>(); 
        foreach (var item in _inventory.Items)
        {
            var config = _gameDataBaae.GetItemConfig(item.ID);
            if (config != null && config.Type == ItemType.Exp && config.Exp != null && item.Quantity > 0)
            {
                expItems.Add(new Tuple<string, int, int>(item.ID, config.Exp.Value, item.Quantity));
            }
        }

        expItems.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        int currentExp = 0;

        foreach (var expItem in expItems)
        {
            string id = expItem.Item1;
            int expValue = expItem.Item2;
            int availableQuantity = expItem.Item3;

            int amountNeeded = (expNeeded - currentExp) / expValue;
            int useAmount = Math.Min(amountNeeded, availableQuantity);
            
            if (useAmount > 0)
            {
                selectedItems.Add(id, useAmount);
                currentExp += useAmount * expValue;
            }
        }

        if (currentExp < expNeeded)
        {
            expItems.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            foreach (var expItem in expItems)
            {
                string id = expItem.Item1;
                int expValue = expItem.Item2;
                int availableQuantity = expItem.Item3;

                int usedSoFar = selectedItems.ContainsKey(id) ? selectedItems[id] : 0;
                if (usedSoFar < availableQuantity)
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

    public bool AutoLevelUp(int targetLevel)
    {
        if (targetLevel <= SaveData.Level) return false;

        int totalCoinsNeeded = Utility.GetCoinNeedToUpgradeCacultivate(targetLevel) - Utility.GetCoinNeedToUpgradeCacultivate(SaveData.Level);
        if (_currency.GetQuantityCurrecy(CurrencyType.Coin) < totalCoinsNeeded) {
            return false;
        }

        int neededExp = GetExpNeededForTargetLevel(targetLevel);
        
        // Nếu đã đủ EXP (do dư thừa từ trước) thì chỉ việc gọi AddExp để nó process vòng lặp trừ Coin và lên cấp!
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
            if(success)
            {
                var config = _gameDataBaae.GetItemConfig(kvp.Key);
                totalGrantedExp += config.Exp.Value * kvp.Value;
            }
        }

        if(totalGrantedExp > 0)
        {
            AddExp(totalGrantedExp);
            return true;
        }

        return false;
    }

    public bool EquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);

        if(weaponSave == null) return false;

        string previousOwnerID = weaponSave.Equip;
        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapWeapon?.Invoke(previousOwnerID, "");
        }

        var weaponConfig = _gameDataBaae.GetItemConfig(weaponSave.TemplateID);
        var runtimeWeapon = EquipmentFactory.CreateWeaponData(weaponSave, weaponConfig);

        Equipment.Equip(runtimeWeapon);

        SaveData.Weapon = itemUUID;

        weaponSave.Equip = SaveData.ID;

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
        
        return true;
    }

    public void UnEquipWeapon(string itemUUID)
    {
        var weaponSave = _inventory.GetWeapon(itemUUID);

        if (string.IsNullOrEmpty(SaveData.Weapon)) return;

        Equipment.Unequip(EquipSlot.Weapon);

        SaveData.Weapon = "";
        weaponSave.Equip = "";

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public bool EquipArmor(string itemUUID)
    {
        var armorSave = _inventory.GetArmor(itemUUID);
        if(armorSave == null) return false;

        string previousOwnerID = armorSave.Equip;
        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapArmor?.Invoke(previousOwnerID, itemUUID, "");
        }

        var armorConfig = _gameDataBaae.GetItemConfig(armorSave.TemplateID);
        var runtimeArmor = EquipmentFactory.CreateArmorData(armorSave, armorConfig);

        Equipment.Equip(runtimeArmor);

        PartSaveData part = new PartSaveData
        {
            ID = itemUUID,
            Type = armorConfig.Armor.Part,
        };

        if(!SaveData.Armors.Contains(part))
        {
            SaveData.Armors.Add(part);
        }

        armorSave.Equip = SaveData.ID;

        // Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();

        return true;
    }

    public bool UnequipArmor(string itemUUID)
    {
        PartSaveData equippedPart = SaveData.Armors.Find(part => part.ID == itemUUID);

        if(equippedPart == null) return false;

        var armorSave = _inventory.GetArmor(itemUUID);
        if(armorSave != null)
        {
            armorSave.Equip = "";
        }

        var slotType = EquipmentFactory.ConvertPartToSlot(equippedPart.Type);

        Equipment.Unequip(slotType);

        SaveData.Armors.Remove(equippedPart);

        //Handle event
        OnEquipmentChanged?.Invoke();
        OnStatsChanged?.Invoke();

        return true;
    }

    public bool ChangeArmor(string newArmorUUID)
    { 
        var newArmorSave = _inventory.GetArmor(newArmorUUID);

        if(newArmorSave == null) return false;

        if (SaveData.Armors.Exists(p => p.ID == newArmorUUID)) return true;

        var armorConfig = _gameDataBaae.GetItemConfig(newArmorSave.TemplateID);
        var targetPart = armorConfig.Armor.Part;

        //Tìm xem trên người mình có đang mặc món nào cùng bộ phận đó không
        PartSaveData myOldPart = SaveData.Armors.Find(p => p.Type == targetPart);
        string myOldArmorUUID = myOldPart != null ? myOldPart.ID : "";

        string previousOwnerID = newArmorSave.Equip;

        // háo món đồ cũ của mình ra trước
        if (!string.IsNullOrEmpty(myOldArmorUUID))
        {
            UnequipArmor(myOldArmorUUID);
        }

        // 5. XỬ LÝ HOÁN ĐỔI ("Cướp" đồ của đồng đội)
        if (!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            // Bắn Event hô to: "Ê hệ thống, lột cái {targetPart} của thằng kia ra, và bắt nó mặc cái {myOldArmorUUID} này vào!"
            UIEvent.OnRequestSwapArmor?.Invoke(previousOwnerID, newArmorUUID, myOldArmorUUID);
        }

        return EquipArmor(newArmorUUID);
    }

    public void UnequipAllArmors()
    {
        if (SaveData.Armors == null || SaveData.Armors.Count == 0) return;

        bool hasChanges = false;
        var equippedArmors = new List<PartSaveData>(SaveData.Armors);

        foreach (var part in equippedArmors)
        {
            var armorSave = _inventory.GetArmor(part.ID);
            if (armorSave != null)
            {
                armorSave.Equip = "";
            }

            var slotType = EquipmentFactory.ConvertPartToSlot(part.Type);
            Equipment.Unequip(slotType);

            hasChanges = true;
        }

        SaveData.Armors.Clear();

        if (hasChanges)
        {
            // Handle event
            OnEquipmentChanged?.Invoke();
            OnStatsChanged?.Invoke();
        }
    }

    public bool ChangeWeapon(string newWeaponUUID)
    {
        if(SaveData.Weapon == newWeaponUUID) return false;
        
        var newWeaponSave = _inventory.GetWeapon(newWeaponUUID);
        if(newWeaponSave == null) return false;

        string myOldWeaponUUID = SaveData.Weapon;
        string previousOwnerID = newWeaponSave.Equip;

        UnEquipWeapon(myOldWeaponUUID);


        if(!string.IsNullOrEmpty(previousOwnerID) && previousOwnerID != SaveData.ID)
        {
            UIEvent.OnRequestSwapWeapon?.Invoke(previousOwnerID, myOldWeaponUUID);
        }

        return EquipWeapon(newWeaponUUID);
    }
    public bool UpgradeSkill(string skillID)
    {
        return false;
    }

    // -------------------------------------------------------------
    // LOGIC ASCENSION ( ĐỘT PHÁ / PHÁ VỠ GIỚI HẠN LEVEL )
    // -------------------------------------------------------------

    public bool GetNextAscensionRequirements(out int nextTier, out int requiredLevel, out List<CostIteam> requiredItems, out int requiredCoin)
    {
        nextTier = SaveData.AscensionTier + 1;
        requiredLevel = 0;
        requiredItems = new List<CostIteam>();
        requiredCoin = 0;

        var characterConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);

        var ascensionConfig = _gameDataBaae.GetAscensionConfig(Utility.GetAscentionConfigIDByCharacterRare(characterConfig.Rare));
        if (ascensionConfig == null) return false;

        if (nextTier > ascensionConfig.MaxTier) 
            return false; // Đã đạt cấp đột phá tối đa

        if (ascensionConfig.TierConfigs.TryGetValue(nextTier.ToString(), out TierConfig tierConfig))
        {
            requiredLevel = tierConfig.LevelRequire;

            // Tách riêng Coin ra khỏi danh sách nguyên liệu
            requiredItems = tierConfig.costs.Where(c => !c.ID.Equals("Coin", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var coinCost = tierConfig.costs.FirstOrDefault(c => c.ID.Equals("Coin", StringComparison.OrdinalIgnoreCase));
            requiredCoin = coinCost != null ? coinCost.Quantity : Utility.GetCoinNeedToAscendCharacter(nextTier);

            return SaveData.Level >= requiredLevel;
        }

        return false;
    }

    public bool IsEligibleForNextAscension()
    {
        if (GetNextAscensionRequirements(out int nextTier, out int requiredLevel, out var requiredItems, out int requiredCoin))
        {
            return SaveData.Level >= requiredLevel;
        }
        return false;
    }

    public bool Ascend()
    {
        if (!IsEligibleForNextAscension()) return false;

        if (!GetNextAscensionRequirements(out int nextTier, out int requiredLevel, out var requiredItems, out int requiredCoin))
        {
            return false;
        }

        // Kiểm tra tiền
        if (_currency.GetQuantityCurrecy(CurrencyType.Coin) < requiredCoin) return false;

        // Kiểm tra nguyên liệu
        foreach (var cost in requiredItems)
        {
            if (_inventory.GetItemQuantity(cost.ID) < cost.Quantity) return false;
        }

        // Trừ tiền
        _currency.Spend(CurrencyType.Coin, requiredCoin);

        // Trừ nguyên liệu
        foreach (var cost in requiredItems)
        {
            _inventory.ConsumeStackableItem(cost.ID, cost.Quantity);
        }

        // Tăng bậc đột phá
        SaveData.AscensionTier = nextTier;

        // Bắn sự kiện thay đổi
        OnLevelChanged?.Invoke();
        OnStatsChanged?.Invoke();

        return true;
    }

    public float GetBaseStat(StatType type)
    {
        float baseValue = 0f;

        if(_baseConfig.Stats != null)
        {
            baseValue = _baseConfig.GetStat(type);
        }

        float growth = baseValue * 0.1f * (SaveData.Level - 1);

        return baseValue + growth;
    }

    public float GetTotalStat(StatType type)
    {
        float baseStat = GetBaseStat(type);

        float flatBonus = Equipment.GetTotalConstantBonus(type);

        float percentBonus = Equipment.GetTotalPercentBonus(type) / 100;

        float totalStat = (baseStat + flatBonus) * (1 + percentBonus);

        return totalStat;
    }

    public float GetTotalArmorConstantStat(StatType type)
    {
        float total = 0f;

        foreach (var kvp in Equipment.EquippedItems)
        {
            EquipSlot slot = kvp.Key;
            EquipmentData item = kvp.Value;

            if (slot == EquipSlot.Weapon) continue;

            foreach (var mod in item.Modifiers)
            {
                if (mod.Type == type && mod.ModifierType == ModifyType.Constant)
                {
                    total += mod.TotalValue;
                }
            }
        }
        return total;
    }

    // -------------------------------------------------------------
    // LOGIC THĂNG SAO NHÂN VẬT (STAR UP)
    // -------------------------------------------------------------

    public bool GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQuantity)
    {
        nextTier = SaveData.StarUp + 1;
        requiredCoin = 0;
        requiredQuantity = 0;

        var characterConfig = _gameDataBaae.GetCharacterConfig(SaveData.ID);
        var starUpConfig = _gameDataBaae.GetStarUpConfig(Utility.GetStarUpConfigIDByCharacterRare(characterConfig.Rare));

        if (starUpConfig == null) return false;

        if (nextTier > starUpConfig.MaxTier) return false;

        if (starUpConfig.Tiers.TryGetValue(nextTier.ToString(), out StarUpTierConfig tierConfig))
        {
            requiredCoin = tierConfig.CostCoin;
            requiredQuantity = tierConfig.Quantity;
            return true;
        }

        return false;
    }

    public bool IsEligibleForNextStarUp()
    {
        if (GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQuantity))
        {
            return _currency.GetQuantityCurrecy(CurrencyType.Coin) >= requiredCoin &&
                   _inventory.GetItemQuantity(SaveData.ID) >= requiredQuantity;
        }
        return false;
    }

    public bool StarUp()
    {
        if (!IsEligibleForNextStarUp()) return false;

        if (!GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQuantity))
        {
            return false;
        }

        // Trừ Coin
        _currency.Spend(CurrencyType.Coin, requiredCoin);

        // Trừ nguyên liệu (Mảnh nhân vật có ID trùng với ID nhân vật)
        _inventory.ConsumeStackableItem(SaveData.ID, requiredQuantity);

        // Nâng sao
        SaveData.StarUp = nextTier;

        // Bắn event
        OnStatsChanged?.Invoke();

        return true;
    }

    public List<SetBonusConfig> GetSetBonusActive()
    {
        return Equipment.GetActiveSetBonuses();
    }

    public void QuickEquipArmor()
    {
        List<ArmorSaveData> bestAvailableArmors = _inventory.GetBestArmorsToEquip();

        if (bestAvailableArmors == null || bestAvailableArmors.Count == 0) return;

        foreach (var newArmor in bestAvailableArmors)
        {
            var newConfig = _gameDataBaae.GetItemConfig(newArmor.TemplateID);
            var targetPart = newConfig.Armor.Part;

            // 2. Tìm xem nhân vật hiện tại đang mặc đồ gì ở vị trí (Part) này
            PartSaveData currentPart = SaveData.Armors.Find(p => p.Type == targetPart);

            // 3. Nếu đang mặc một món đồ nào đó, ta tiến hành so sánh
            if (currentPart != null)
            {
                var currentArmor = _inventory.GetArmor(currentPart.ID);

                if (currentArmor != null)
                {
                    var currentConfig = _gameDataBaae.GetItemConfig(currentArmor.TemplateID);

                    // So sánh độ hiếm (Rarity) trước
                    int rarityComparison = newConfig.Rarity.CompareTo(currentConfig.Rarity);

                    // So sánh cấp độ (Level) sau
                    int levelComparison = newArmor.Level.CompareTo(currentArmor.Level);

                    // Nếu món đồ mới CÙI HƠN hoặc BẰNG món đồ đang mặc -> Bỏ qua, không thay!
                    if (rarityComparison < 0 || (rarityComparison == 0 && levelComparison <= 0))
                    {
                        continue;
                    }
                }
            }

            bool success = ChangeArmor(newArmor.UUID);

        }


    }
}
