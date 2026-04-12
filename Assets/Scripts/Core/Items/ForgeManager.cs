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
        if (level >= Definition.CharacterMaxLevel) return false; // Thường giới hạn cấp vũ khí bằng cấp nhân vật hoặc 100

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
        int maxLevel = Definition.CharacterMaxLevel;

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
        int maxLevel = Definition.CharacterMaxLevel;

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
}
