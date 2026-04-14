using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeManager
{
    private CurrencyManager _currencyManager;
    private InventoryManager _inventoryManager;

    public ForgeManager(CurrencyManager currencyManager, InventoryManager inventoryManager)
    {
        _currencyManager = currencyManager;
        _inventoryManager = inventoryManager;
    }

    /// <summary>
    /// Thực hiện nâng cấp vũ khí lên 1 cấp
    /// </summary>
    public bool UpgradeWeapon(string weaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        if (weaponSave == null) return false;

        int level = weaponSave.CurrentLevel;
        if (level >= Definition.MAX_CHARACTER_LEVEL) return false; // Thường giới hạn cấp vũ khí bằng cấp nhân vật hoặc 100

        int coinNeeded = Utility.GetCoinNeedToUpgradeWeapon(level + 1) - Utility.GetCoinNeedToUpgradeWeapon(level);
        
        // Theo yêu cầu, Essence sẽ được trừ bằng số cần thiết để lên cấp tiếp theo.
        // Công thức chuẩn thường là hiệu số, UI báo lỗi gõ thiếu nhưng logic ở đây dùng hiệu số
        int essenceNeeded = Utility.GetEssenceNeedToUpgradeWeapon(level + 1) - Utility.GetEssenceNeedToUpgradeWeapon(level);

        // Kiểm tra xem có đủ tài nguyên không
        if (_currencyManager.GetQuantityCurrecy(CurrencyType.Coin) < coinNeeded) return false;
        if (_currencyManager.GetQuantityCurrecy(CurrencyType.RelicEssence) < essenceNeeded) return false;

        // Trừ tiền và nguyên liệu
        _currencyManager.Spend(CurrencyType.Coin, coinNeeded);
        _currencyManager.Spend(CurrencyType.RelicEssence, essenceNeeded);

        // Nâng cấp vũ khí
        weaponSave.CurrentLevel++;

        // Thông báo UI cập nhật (nếu cần thiết)
        UIEvent.OnSlelectWeaponEnchance?.Invoke(weaponUUID);

        UIEvent.OnEquipmentUpgraded?.Invoke(weaponUUID);

        return true;
    }

    /// <summary>
    /// Thực hiện nâng cấp vũ khí tối đa cấp độ dựa trên số Coin và Essence đang có
    /// </summary>
    public int UpgradeWeaponMax(string weaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        if (weaponSave == null) return 0;

        int currentLevel = weaponSave.CurrentLevel;
        int maxLevel = Definition.MAX_CHARACTER_LEVEL;

        int availableCoin = _currencyManager.GetQuantityCurrecy(CurrencyType.Coin);
        int availableEssence = _currencyManager.GetQuantityCurrecy(CurrencyType.RelicEssence);

        int upgradesDone = 0;
        int totalCoinSpent = 0;
        int totalEssenceSpent = 0;

        while (currentLevel < maxLevel)
        {
            int coinNeeded = Utility.GetCoinNeedToUpgradeWeapon(currentLevel + 1) - Utility.GetCoinNeedToUpgradeWeapon(currentLevel);
            int essenceNeeded = Utility.GetEssenceNeedToUpgradeWeapon(currentLevel + 1) - Utility.GetEssenceNeedToUpgradeWeapon(currentLevel);

            if (availableCoin >= coinNeeded && availableEssence >= essenceNeeded)
            {
                availableCoin -= coinNeeded;
                availableEssence -= essenceNeeded;

                totalCoinSpent += coinNeeded;
                totalEssenceSpent += essenceNeeded;

                currentLevel++;
                upgradesDone++;
            }
            else
            {
                break; // Không đủ tài nguyên để nâng thêm
            }
        }

        if (upgradesDone > 0)
        {
            _currencyManager.Spend(CurrencyType.Coin, totalCoinSpent);
            _currencyManager.Spend(CurrencyType.RelicEssence, totalEssenceSpent);
            weaponSave.CurrentLevel = currentLevel;

            UIEvent.OnSlelectWeaponEnchance?.Invoke(weaponUUID);
            UIEvent.OnEquipmentUpgraded?.Invoke(weaponUUID);
        }

        return upgradesDone; // Trả về số cấp đã nâng
    }

    /// <summary>
    /// Lấy danh sách các vũ khí giống với vũ khí hiện tại (trùng TemplateID), dùng để hiển thị và làm nguyên liệu đột phá.
    /// </summary>
    public List<WeaponSaveData> GetDuplicateWeapons(string weaponUUID)
    {
        var duplicates = new List<WeaponSaveData>();
        var targetWeapon = _inventoryManager.GetWeapon(weaponUUID);

        if (targetWeapon == null) return duplicates;

        foreach (var weapon in _inventoryManager.Weapons)
        {
            // Không lấy chính nó và chỉ lấy những vũ khí cùng TemplateID, chưa được trang bị
            if (weapon.UUID != targetWeapon.UUID && weapon.TemplateID == targetWeapon.TemplateID && string.IsNullOrEmpty(weapon.Equip))
            {
                duplicates.Add(weapon);
            }
        }

        return duplicates;
    }

    /// <summary>
    /// Đột phá (Ascend/Upgrade) vũ khí lên 1 sao/cấp độ đột phá (Tự động chọn phôi đầu tiên)
    /// </summary>
    public bool AscendWeapon(string weaponUUID)
    {
        var duplicates = GetDuplicateWeapons(weaponUUID);
        if (duplicates.Count == 0) return false;

        return AscendWeapon(weaponUUID, duplicates[0].UUID);
    }

    /// <summary>
    /// Đột phá (Ascend/Upgrade) vũ khí lên 1 sao bằng cách tiêu thụ 1 vũ khí phôi chỉ định
    /// </summary>
    public bool AscendWeapon(string weaponUUID, string consumeWeaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        var consumeWeapon = _inventoryManager.GetWeapon(consumeWeaponUUID);

        if (weaponSave == null || consumeWeapon == null) return false;
        if (weaponSave.TemplateID != consumeWeapon.TemplateID) return false;

        int targetUpgrade = weaponSave.CurrentUpgrade + 1;
        int coinNeeded = Utility.GetCoinNeedToAsscendWeapon(targetUpgrade);

        if (_currencyManager.GetQuantityCurrecy(CurrencyType.Coin) < coinNeeded) return false;

        // Trừ tiền và vũ khí nguyên liệu
        _currencyManager.Spend(CurrencyType.Coin, coinNeeded);
        _inventoryManager.RemoveWeapon(consumeWeaponUUID);
        
        weaponSave.CurrentUpgrade++;

        UIEvent.OnSlelectWeaponEnchance?.Invoke(weaponUUID);
        UIEvent.OnEquipmentUpgraded?.Invoke(weaponUUID);
        return true;
    }

    /// <summary>
    /// Trả về chuỗi định dạng số Coin và Essence cần thiết để nâng cấp (dành cho UI hiển thị).
    /// </summary>
    public (string coinStr, string essenceStr) GetUpgradeRequirementsFormatted(string weaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        if (weaponSave == null) return ("0", "0");

        int level = weaponSave.CurrentLevel;
        
        string coinFormatted = Utility.FormatCurrency(Utility.GetCoinNeedToUpgradeWeapon(level + 1) - Utility.GetCoinNeedToUpgradeWeapon(level));
        
        // Theo đúng công thức yêu cầu của bạn:
        string essenceFormatted = Utility.FormatCurrency(Utility.GetEssenceNeedToUpgradeWeapon(level + 1) - Utility.GetEssenceNeedToUpgradeWeapon(level));

        return (coinFormatted, essenceFormatted);
    }

    /// <summary>
    /// Trả về chuỗi định dạng số Coin và Essence cần thiết để nâng cấp MAX (dành cho UI hiển thị).
    /// </summary>
    public (int levelsUp, string coinStr, string essenceStr) GetMaxUpgradeRequirementsFormatted(string weaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        if (weaponSave == null) return (0, "0", "0");

        int currentLevel = weaponSave.CurrentLevel;
        int maxLevel = Definition.MAX_CHARACTER_LEVEL;

        int availableCoin = _currencyManager.GetQuantityCurrecy(CurrencyType.Coin);
        int availableEssence = _currencyManager.GetQuantityCurrecy(CurrencyType.RelicEssence);

        int upgradesDone = 0;
        int totalCoinSpent = 0;
        int totalEssenceSpent = 0;

        while (currentLevel < maxLevel)
        {
            int coinNeeded = Utility.GetCoinNeedToUpgradeWeapon(currentLevel + 1) - Utility.GetCoinNeedToUpgradeWeapon(currentLevel);
            int essenceNeeded = Utility.GetEssenceNeedToUpgradeWeapon(currentLevel + 1) - Utility.GetEssenceNeedToUpgradeWeapon(currentLevel);

            if (availableCoin >= coinNeeded && availableEssence >= essenceNeeded)
            {
                availableCoin -= coinNeeded;
                availableEssence -= essenceNeeded;
                totalCoinSpent += coinNeeded;
                totalEssenceSpent += essenceNeeded;
                currentLevel++;
                upgradesDone++;
            }
            else
            {
                break;
            }
        }

        string coinFormatted = Utility.FormatCurrency(totalCoinSpent);
        string essenceFormatted = Utility.FormatCurrency(totalEssenceSpent);

        return (upgradesDone, coinFormatted, essenceFormatted);
    }

    /// <summary>
    /// Trả về số tiền cần để đột phá (Ascend)
    /// </summary>
    public string GetAscendRequirementsFormatted(string weaponUUID)
    {
        var weaponSave = _inventoryManager.GetWeapon(weaponUUID);
        if (weaponSave == null) return "0";

        int targetUpgrade = weaponSave.CurrentUpgrade + 1;
        int coinNeeded = Utility.GetCoinNeedToAsscendWeapon(targetUpgrade);

        return Utility.FormatCurrency(coinNeeded);
    }

    // ═══════════════════════════════════════════════════════════════
    // ARMOR UPGRADE SYSTEM
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Nâng cấp armor từ level hiện tại lên targetLevel.
    /// Tự động xử lý substat mở khóa/nâng cấp tại các milestone (3, 6, 9, 12, 15).
    /// </summary>
    public bool UpgradeArmor(string armorUUID, int targetLevel, GameDataBase gameDataBase)
    {
        var armorSave = _inventoryManager.GetArmor(armorUUID);
        if (armorSave == null) return false;

        int currentLevel = armorSave.Level;
        if (targetLevel <= currentLevel || targetLevel > Definition.MAX_ARMOR_LEVEL) return false;

        int coinNeeded = Utility.GetTotalCoinForArmorUpgrade(currentLevel, targetLevel);
        int primoriteNeeded = Utility.GetTotalPrimoriteForArmorUpgrade(currentLevel, targetLevel);

        if (_currencyManager.GetQuantityCurrecy(CurrencyType.Coin) < coinNeeded) return false;
        if (_currencyManager.GetQuantityCurrecy(CurrencyType.ArmorPrimorite) < primoriteNeeded) return false;

        // Trừ nguyên liệu
        _currencyManager.Spend(CurrencyType.Coin, coinNeeded);
        _currencyManager.Spend(CurrencyType.ArmorPrimorite, primoriteNeeded);

        // Xử lý substat tại các milestone
        var config = gameDataBase.GetItemConfig(armorSave.TemplateID);

        if (config != null && config.Armor != null)
        {
            var poolConfig = gameDataBase.GetSubstatPoolConfig(config.Armor.SubstatPoolID);
            ProcessSubStats(armorSave, poolConfig, currentLevel, targetLevel);
        }

        // Cập nhật level
        armorSave.Level = targetLevel;

        // Thông báo UI
        UIEvent.OnArmorUpgraded?.Invoke(armorUUID);

        return true;
    }

    /// <summary>
    /// Xử lý mở khóa/nâng cấp substat khi level vượt qua các milestone (mỗi 3 level).
    /// </summary>
    private void ProcessSubStats(ArmorSaveData armorSave, SubstatPoolConfig poolConfig, int fromLevel, int toLevel)
    {
        if (armorSave.Substats == null)
            armorSave.Substats = new List<RolledSubStat>();

        for (int lv = fromLevel + 1; lv <= toLevel; lv++)
        {
            // Kiểm tra milestone: level chia hết cho ARMOR_SUBSTAT_INTERVAL (3, 6, 9, 12, 15)
            if (lv % Definition.ARMOR_SUBSTAT_INTERVAL == 0)
            {
                if (armorSave.Substats.Count < Definition.MAX_ARMOR_SUBSTATS)
                {
                    // Mở khóa substat mới
                    RollNewSubStat(armorSave, poolConfig);
                }
                else
                {
                    // Nâng cấp 1 substat ngẫu nhiên
                    UpgradeRandomSubStat(armorSave, poolConfig);
                }
            }
        }
    }

    /// <summary>
    /// Random 1 substat mới từ pool, không trùng substat đã có.
    /// </summary>
    private void RollNewSubStat(ArmorSaveData armorSave, SubstatPoolConfig poolConfig)
    {

        if (poolConfig == null || poolConfig.Pools.Count == 0) return;

        // Lọc pool: bỏ các stat đã có
        var existingTypes = new HashSet<StatType>();
        foreach (var sub in armorSave.Substats)
        {
            existingTypes.Add(sub.Type);
        }

        // bỏ cả main task
        //existingTypes.Add(armorConfig.MainStat.Type);

        var availablePool = new List<SubstatCompoment>();
        foreach (var pool in poolConfig.Pools)
        {
            if (!existingTypes.Contains(pool.Type))
            {
                availablePool.Add(pool);
            }
        }

        if (availablePool.Count == 0) return;

        // Random chọn 1 stat từ pool
        int randomIndex = Random.Range(0, availablePool.Count);
        var chosen = availablePool[randomIndex];

        // Random giá trị trong [Min, Max]
        int rolledValue = UnityEngine.Mathf.RoundToInt(
            UnityEngine.Random.Range(chosen.Min, chosen.Max)
        );

        var newSub = new RolledSubStat(chosen.Type, rolledValue, chosen.ModifierType);
        armorSave.Substats.Add(newSub);
    }

    /// <summary>
    /// Nâng cấp 1 substat ngẫu nhiên trong danh sách hiện có.
    /// </summary>
    private void UpgradeRandomSubStat(ArmorSaveData armorSave, SubstatPoolConfig poolConfig)
    {
        if (armorSave.Substats.Count == 0) return;

        // Chọn ngẫu nhiên 1 substat
        int randomIndex = Random.Range(0, armorSave.Substats.Count);
        var subToUpgrade = armorSave.Substats[randomIndex];

        // Tìm pool config tương ứng để lấy range bonus
        SubstatCompoment matchingPool = null;
        if (poolConfig.Pools != null)
        {
            foreach (var pool in poolConfig.Pools)
            {
                if (pool.Type == subToUpgrade.Type)
                {
                    matchingPool = pool;
                    break;
                }
            }
        }

        // Tính bonus: random 70%-100% của min value
        int bonusValue;
        if (matchingPool != null)
        {
            bonusValue = Mathf.RoundToInt(
                Random.Range(matchingPool.Min * 0.7f, matchingPool.Min)
            );
        }
        else
        {
            bonusValue = 1; // Fallback
        }

        if (bonusValue < 1) bonusValue = 1;
        subToUpgrade.Upgrade(bonusValue);
    }

    /// <summary>
    /// Tính chi phí nâng cấp armor từ fromLevel lên toLevel.
    /// </summary>
    public (int coin, int primorite) GetUpgradeArmorCost(int fromLevel, int toLevel)
    {
        int coin = Utility.GetTotalCoinForArmorUpgrade(fromLevel, toLevel);
        int primorite = Utility.GetTotalPrimoriteForArmorUpgrade(fromLevel, toLevel);
        return (coin, primorite);
    }

    /// <summary>
    /// Trả về lượng ArmorPrimorite nhận được khi quy đổi armor.
    /// </summary>
    public int GetSalvagePrimoriteValue(string armorUUID)
    {
        var armorSave = _inventoryManager.GetArmor(armorUUID);
        if (armorSave == null) return 0;
        return Utility.GetArmorPrimoriteFromSalvage(armorSave.Rare, armorSave.Level);
    }

    /// <summary>
    /// Quy đổi 1 armor thành ArmorPrimorite. Armor đang equipped sẽ bị từ chối.
    /// </summary>
    public bool SalvageArmor(string armorUUID)
    {
        var armorSave = _inventoryManager.GetArmor(armorUUID);
        if (armorSave == null) return false;
        if (!string.IsNullOrEmpty(armorSave.Equip)) return false; // Đang mặc, không được quy đổi

        int primorite = Utility.GetArmorPrimoriteFromSalvage(armorSave.Rare, armorSave.Level);

        _inventoryManager.RemoveArmor(armorUUID);
        _currencyManager.Add(CurrencyType.ArmorPrimorite, primorite);

        return true;
    }

    /// <summary>
    /// Quy đổi nhiều armor cùng lúc thành ArmorPrimorite.
    /// </summary>
    public int SalvageArmors(System.Collections.Generic.List<string> armorUUIDs)
    {
        int totalSalvaged = 0;
        foreach (var uuid in armorUUIDs)
        {
            if (SalvageArmor(uuid))
            {
                totalSalvaged++;
            }
        }
        return totalSalvaged;
    }
}

