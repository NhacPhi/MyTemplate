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
}
