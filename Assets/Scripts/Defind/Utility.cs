using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine;

public static class Utility 
{
   public const float MAX_STAT_VALUE = 1000000;
   public static string GenarateID()
    {
        return Guid.NewGuid().ToString();
    }

    public static string GetArmorPartName(ArmorPart part)
    {
        switch(part)
        {
            case ArmorPart.Helmet:
                return LocalizationManager.Instance.GetLocalizedValue("STR_HELMET");
            case ArmorPart.Chestplate:
                return LocalizationManager.Instance.GetLocalizedValue("STR_CHESTPLATE");
            case ArmorPart.Gloves:
                return LocalizationManager.Instance.GetLocalizedValue("STR_GLOVES");
            case ArmorPart.Boots:
                return LocalizationManager.Instance.GetLocalizedValue("STR_BOOTS");
            case ArmorPart.Belt:
                return LocalizationManager.Instance.GetLocalizedValue("STR_BELT");
            case ArmorPart.Ring:
                return LocalizationManager.Instance.GetLocalizedValue("STR_RING");
        }
        return "";
    }

    public static string GetArmorRaretName(Rare rare)
    {
        switch (rare)
        {
            case Rare.Common:
                return LocalizationManager.Instance.GetLocalizedValue("STR_COMMON_ARMOR");
            case Rare.Uncommon:
                return LocalizationManager.Instance.GetLocalizedValue("STR_UNCOMMON_ARMOR");
            case Rare.Rare:
                return LocalizationManager.Instance.GetLocalizedValue("STR_RARE_ARMOR");
            case Rare.Epic:
                return LocalizationManager.Instance.GetLocalizedValue("STR_EPIC_ARMOR");
            case Rare.Legendary:
                return LocalizationManager.Instance.GetLocalizedValue("STR_LEGENDARY_ARMOR");
        }
        return "";
    }

    // Stat Growth level
    //Stat(level)=Base+Growth×(level-1)×(0.7+0.03×(level-1))
    public static int GetCharacterExpByLevel(int level)
    {
        return 1800 + 1000 * (level - 1) + 600 * (level - 1) * (level - 1);
    }

    public static string GetExpConfigIDByCharacterRare(CharacterRare rare)
    {
        switch(rare)
        {
            case CharacterRare.R: return "Curve_R";
            case CharacterRare.SR: return "Curve_SR";
            case CharacterRare.SSR: return "Curve_SSR";
            case CharacterRare.UR: return "Curve_UR";
        }
        return "";
    }

    public static string GetStarUpConfigIDByCharacterRare(CharacterRare rare)
    {
        switch(rare)
        {
            case CharacterRare.R: return "starup_r";
            case CharacterRare.SR: return "starup_sr";
            case CharacterRare.SSR: return "starup_ssr";
            case CharacterRare.UR: return "starup_ur";
        }
        return "";
    }

    public static string GetContextByStatType(StatType type)
    {
        string locailzationID = "";

        switch(type)
        {
            case StatType.ATK: locailzationID = "UI_ATK"; break;
            case StatType.HP: locailzationID = "UI_HP"; break;
            case StatType.DEF: locailzationID = "UI_DEF"; break;
            case StatType.SPEED: locailzationID = "UI_SPD"; break;
            case StatType.CRIT_RATE: locailzationID = "UI_CRIT_RATE"; break;
            case StatType.CRIT_DMG: locailzationID = "UI_CRIT_DMG"; break;
            case StatType.PEN: locailzationID = "UI_PENETRATION"; break;
            case StatType.CRIT_DMG_RES: locailzationID = "UI_CRIT_DMG_RES"; break;
        }

        return LocalizationManager.Instance.GetLocalizedValue(locailzationID);
    }

    public static string GetConvertStatValueToString(float value, ModifyType type)
    {
        if (type == ModifyType.Percent)
        {
            return value.ToString() + "%";
        }
        else
        {
            return value+ "%";
        }
    }

    public static string GetAscentionConfigIDByCharacterRare(CharacterRare rare)
    {
        switch (rare)
        {
            case CharacterRare.R: return "ascension_r";
            case CharacterRare.SR: return "ascension_sr";
            case CharacterRare.SSR: return "ascension_ssr";
            case CharacterRare.UR: return "ascension_ur";
        }
        return "";
    }
    //Exp to upgrade
    //ExpRequired(n)=1800+1000×(n-1)+600×(n-1)2
    public static int GetStatGrowthLevel(int level, float growth)
    {
        return Convert.ToInt32(growth * (level - 1) * (0.7f + 0.03f * (level - 1)));
    }

    //Coin to upgrade
    //Cost(level)=BaseCost+Growth×(level−1)2
    public static int GetCoinNeedToUpgradeCacultivate(int level)
    {
        return 3000 + 2000 * (level - 1) * (level - 1);
    }

    public static int GetShardNeedToUpgradeAscend(int boostStat )
    {
        int boost = (boostStat - 1) / 3;
        return 60 + 60 * boost;
    }
    public static int GetCoinNeedToAscendCharacter(int boostStat)
    {
        return 8000 + 4000 * (boostStat - 1) * (boostStat - 1);
    }

    public static int GetEssenceNeedToUpgradeWeapon(int level)
    {
        return 4200 + 320 * (level - 1) * (level - 1);
    }

    public static int GetMaxLevelWithEssence(int availableEssence)
    {
        if (availableEssence < 4200)
        {
            return 0;
        }

        float calculatedLevel = 1f + Mathf.Sqrt((availableEssence - 4200f) / 320f);

        return Mathf.FloorToInt(calculatedLevel);
    }

    public static int GetCoinNeedToUpgradeWeapon(int level)
    {
        return 5000 + 480 * (level - 1) * (level - 1);
    }

    public static int GetCoinNeedToAsscendWeapon(int level)
    {
        return 12000 + 6000 * (level - 1) * (level - 1);
    }

    public static string FormatCurrency(int amount)
    {
        if (amount >= 1000000000)
            return (amount / 1000000000f).ToString("0.##") + "B";
        if (amount >= 1000000)
            return (amount / 1000000f).ToString("0.##") + "M";
        
        return amount.ToString();
    }

    // ═══════════════════════════════════════
    // Armor Upgrade Formulas
    // ═══════════════════════════════════════

    /// <summary>
    /// Tính coin cần để nâng cấp armor lên level chỉ định.
    /// Cost(level) = 2000 + 800 × (level - 1)²
    /// </summary>
    public static int GetCoinNeedToUpgradeArmor(int level)
    {
        return 2000 + 800 * (level - 1) * (level - 1);
    }

    /// <summary>
    /// Tính ArmorPrimorite cần để nâng cấp armor lên level chỉ định.
    /// Cost(level) = 500 + 200 × (level - 1)²
    /// </summary>
    public static int GetPrimoriteNeedToUpgradeArmor(Rare rare, int level)
    {
        // Level 1 là mặc định khi nhận đồ, không tốn phí nâng cấp
        if (level <= 1) return 0;

        // 1. Hệ số nhân dựa trên độ hiếm (Đồ càng hiếm nâng càng đắt)
        float rareMultiplier = rare switch
        {
            Rare.Common => 1.0f,
            Rare.Uncommon => 1.5f,
            Rare.Rare => 2.0f,
            Rare.Epic => 3.0f,
            Rare.Legendary => 5.0f,
            _ => 1.0f
        };

        // 2. Công thức gốc: Level càng cao tốn càng nhiều (Mốc đầu 500, mỗi cấp tăng 200)
        int baseCost = 500 + 200 * (level - 1) * (level - 1);

        // 3. Nhân hệ số và làm tròn
        return Mathf.RoundToInt(baseCost * rareMultiplier);
    }

    /// <summary>
    /// Tính tổng coin cần để nâng từ fromLevel lên toLevel.
    /// </summary>
    public static int GetTotalCoinForArmorUpgrade(int fromLevel, int toLevel)
    {
        int total = 0;
        for (int lv = fromLevel + 1; lv <= toLevel; lv++)
        {
            total += GetCoinNeedToUpgradeArmor(lv) - GetCoinNeedToUpgradeArmor(lv - 1);
        }
        return total;
    }

    /// <summary>
    /// Tính tổng ArmorPrimorite cần để nâng từ fromLevel lên toLevel.
    /// </summary>
    public static int GetTotalPrimoriteForArmorUpgrade(int fromLevel, int toLevel, Rare rare)
    {
        int total = 0;
        for (int lv = fromLevel + 1; lv <= toLevel; lv++)
        {
            total += GetPrimoriteNeedToUpgradeArmor(rare, lv) - GetPrimoriteNeedToUpgradeArmor(rare, lv - 1);
        }
        return total;
    }

    /// <summary>
    /// Tính ArmorPrimorite nhận được khi quy đổi (salvage) armor.
    /// BaseValue × RarityMultiplier + 50 × Level
    /// </summary>
    public static int GetArmorPrimoriteFromSalvage(Rare rare, int level)
    {
        // 1. Giá trị gốc của phôi (Tùy theo độ hiếm)
        int baseValue = rare switch
        {
            Rare.Common => 100,
            Rare.Uncommon => 200,
            Rare.Rare => 400,
            Rare.Epic => 800,
            Rare.Legendary => 1600,
            _ => 100
        };

        // Nếu đồ chưa nâng cấp gì (Level 1), chỉ trả lại giá trị gốc
        if (level <= 1) return baseValue;

        // 2. Tính tổng tài nguyên đã tiêu tốn từ Level 1 đến Level hiện tại
        int totalInvested = GetPrimoriteNeedToUpgradeArmor(rare, level);

        // 3. Hoàn trả 80% số nguyên liệu đã nâng cấp + Giá trị gốc
        return baseValue + Mathf.RoundToInt(totalInvested * 0.8f);
    }

    /// <summary>
    /// Tính main stat của armor theo level.
    /// MainStat(level) = baseValue + baseValue × 0.12 × level
    /// </summary>
    public static int GetArmorMainStatByLevel(float baseValue, int level)
    {
        return Convert.ToInt32(baseValue + baseValue * 0.12f * level);
    }

    /// <summary>
    /// Tính enhancement level của từng skill dựa trên star_up (0-6).
    /// Thứ tự cường hóa: Base → Major → Ultimate, mỗi skill tối đa 2 lần.
    /// star_up 0: (0, 0, 0)
    /// star_up 1: (1, 0, 0)
    /// star_up 2: (1, 1, 0)
    /// star_up 3: (1, 1, 1)
    /// star_up 4: (2, 1, 1)
    /// star_up 5: (2, 2, 1)
    /// star_up 6: (2, 2, 2)
    /// </summary>
    public static int GetSkillEnhancementLevel(SkillCharacter skillType, int starUp)
    {
        int skillIndex = skillType switch
        {
            SkillCharacter.Base     => 0,
            SkillCharacter.Major    => 1,
            SkillCharacter.Ultimate => 2,
            _ => 0
        };

        int level = 0;
        // Vòng 1 (star_up 1-3): mỗi skill +1
        if (starUp >= skillIndex + 1) level++;
        // Vòng 2 (star_up 4-6): mỗi skill +1
        if (starUp >= skillIndex + 4) level++;

        return level;
    }
}

