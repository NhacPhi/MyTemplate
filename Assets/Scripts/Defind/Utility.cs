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
    // Stat(level) = Base + Growth × (level-1) × (1.0 + 0.005 × (level-1))
    // Exp required for each level step (from level-1 to level)
    // Total for Lv 1 -> 100 = ~2.8M EXP (~280 supreme_exp books)
    public static int GetCharacterExpByLevel(int level)
    {
        if (level <= 1) return 0;
        return (int)(300 + 120 * (level - 1) + 6.5f * (level - 1) * (level - 1));
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

    public static Rare ConvertCharacterRareToItemRare(CharacterRare rare)
    {
        switch (rare)
        {
            case CharacterRare.UR:
            case CharacterRare.SSR:
                return Rare.Legendary;
            case CharacterRare.SR:
                return Rare.Epic;
            case CharacterRare.R:
            default:
                return Rare.Rare;
        }
    }

    /// <summary>
    /// Số lượng mảnh nhận được khi quy đổi nhân vật đã sở hữu theo CharacterRare
    /// </summary>
    public static int GetDuplicateCharacterShardAmount(CharacterRare rare)
    {
        switch (rare)
        {
            case CharacterRare.UR:
                return 90;
            case CharacterRare.SSR:
                return 60;
            case CharacterRare.SR:
                return 30;
            case CharacterRare.R:
            default:
                return 20;
        }
    }

    /// <summary>
    /// Số lượng mảnh nhận được khi quy đổi nhân vật đã sở hữu theo Item Rare
    /// </summary>
    public static int GetDuplicateCharacterShardAmount(Rare rare)
    {
        switch (rare)
        {
            case Rare.Legendary:
                return 60;
            case Rare.Epic:
                return 30;
            case Rare.Rare:
                return 20;
            case Rare.Uncommon:
            case Rare.Common:
            default:
                return 10;
        }
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
            case StatType.PENETRATION: locailzationID = "UI_PENETRATION"; break;
            case StatType.DEF_SHRED: locailzationID = "UI_DEF_SHRED"; break;
            case StatType.CRIT_DMG_RES: locailzationID = "UI_CRIT_DMG_RES"; break;
        }

        return LocalizationManager.Instance.GetLocalizedValue(locailzationID);
    }

    public static bool IsPercentStat(StatType statType, ModifyType type = ModifyType.Flat)
    {
        if (type == ModifyType.Percent) return true;

        return statType switch
        {
            StatType.CRIT_RATE or
            StatType.CRIT_DMG or
            StatType.PENETRATION or
            StatType.CRIT_DMG_RES or
            StatType.EHR or
            StatType.RES => true,
            _ => false
        };
    }

    public static float GetAppropriateArmorMainBaseValue(StatType statType, ModifyType modType, float fallbackValue)
    {
        if (IsPercentStat(statType, modType))
        {
            if (statType == StatType.CRIT_RATE || statType == StatType.CRIT_DMG) return 2.5f;
            if (statType == StatType.PENETRATION) return 1.5f;
            return 3.5f;
        }
        else
        {
            switch (statType)
            {
                case StatType.SPEED: return 3.0f;
                case StatType.ATK: return 20.0f;
                case StatType.HP: return 100.0f;
                case StatType.DEF: return 15.0f;
                case StatType.DEF_SHRED: return 30.0f;
                default: return fallbackValue > 0 ? fallbackValue : 20.0f;
            }
        }
    }

    public static string GetConvertStatValueToString(float value, ModifyType type, StatType statType = StatType.None)
    {
        if (IsPercentStat(statType, type))
        {
            return value.ToString() + "%";
        }
        else
        {
            return value.ToString();
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

    public static string GetAscentionConfigIDByWeaponRare(Rare rare)
    {
        switch (rare)
        {
            case Rare.Common: return "ascension_common";
            case Rare.Uncommon: return "ascension_uncommon";
            case Rare.Rare: return "ascension_rare";
            case Rare.Epic: return "ascension_epic";
            case Rare.Legendary: return "ascension_legend";
        }
        return "ascension_common";
    }

    // Stat Growth level
    public static int GetStatGrowthLevel(int level, float growth)
    {
        return Convert.ToInt32(growth * (level - 1) * (1.0f + 0.005f * (level - 1)));
    }

    // Cumulative Coin required to level up character from Level 1 to 'level'
    // Total for Lv 1 -> 100 = ~1.45M Coin (smooth progression)
    public static int GetCoinNeedToUpgradeCacultivate(int level)
    {
        if (level <= 1) return 0;
        int l = level - 1;
        return (int)(1000 * l + 80f * l * l + 1.2f * l * l * l);
    }

    public static int GetShardNeedToUpgradeAscend(int boostStat)
    {
        int boost = (boostStat - 1) / 3;
        return 60 + 60 * boost;
    }

    // Coin cost for Character Ascension by Tier (1 to 5)
    public static int GetCoinNeedToAscendCharacter(int tier)
    {
        return tier switch
        {
            1 => 5000,    // Lv 20 -> 40
            2 => 20000,   // Lv 40 -> 60
            3 => 60000,   // Lv 60 -> 80
            4 => 150000,  // Lv 80 -> 90
            5 => 350000,  // Lv 90 -> 100
            _ => 10000 * tier * tier
        };
    }

    // Cumulative Essence required to reach weapon 'level' from level 1
    // Total for Lv 1 -> 100 = ~23,000 Essence (~80-120 dungeon runs)
    public static int GetEssenceNeedToUpgradeWeapon(int level)
    {
        if (level <= 1) return 0;
        int l = level - 1;
        return (int)(15 * l + 2.2f * l * l);
    }

    public static int GetMaxLevelWithEssence(int availableEssence)
    {
        if (availableEssence <= 0) return 1;
        // Solve: 2.2 * l^2 + 15 * l - availableEssence = 0
        float delta = 225f + 8.8f * availableEssence;
        float l = (-15f + Mathf.Sqrt(delta)) / 4.4f;
        int level = 1 + Mathf.FloorToInt(l);
        return Mathf.Clamp(level, 1, Definition.MAX_WEAPON_LEVEL);
    }

    // Cumulative Coin required to reach weapon 'level' from level 1
    // Total for Lv 1 -> 100 = ~642,000 Coin
    public static int GetCoinNeedToUpgradeWeapon(int level)
    {
        if (level <= 1) return 0;
        int l = level - 1;
        return (int)(50 * l + 6.5f * l * l);
    }

    // Coin cost for Weapon Ascension by Tier (1 to 6)
    public static int GetCoinNeedToAsscendWeapon(int tier)
    {
        return tier switch
        {
            1 => 4000,
            2 => 15000,
            3 => 45000,
            4 => 100000,
            5 => 250000,
            6 => 500000,
            _ => 8000 * tier * tier
        };
    }



    public static string FormatCurrency(int amount)
    {
        if (amount >= 1000000000)
            return (amount / 1000000000f).ToString("0.##") + "B";
        if (amount >= 1000000)
            return (amount / 1000000f).ToString("0.##") + "M";
        
        return amount.ToString();
    }

    public static string FormatCurrency(float amount)
    {
        if (amount >= 1000000000f)
            return (amount / 1000000000f).ToString("0.##") + "B";
        if (amount >= 1000000f)
            return (amount / 1000000f).ToString("0.##") + "M";
        
        return Mathf.Approximately(amount, Mathf.Round(amount)) 
            ? ((int)amount).ToString() 
            : amount.ToString("0.##");
    }

    public static string FormatCurrency(double amount)
    {
        if (amount >= 1000000000.0)
            return (amount / 1000000000.0).ToString("0.##") + "B";
        if (amount >= 1000000.0)
            return (amount / 1000000.0).ToString("0.##") + "M";
        
        return Math.Abs(amount - Math.Round(amount)) < 0.0001 
            ? ((long)amount).ToString() 
            : amount.ToString("0.##");
    }

    // ═══════════════════════════════════════
    // Armor Upgrade Formulas
    // ═══════════════════════════════════════

    /// <summary>
    /// Tính coin tích lũy cần để nâng cấp armor lên level chỉ định (1 -> 15).
    /// Lv 1 -> 15 = ~92,400 Coin / món (~554,400 Coin cho bộ 6 món).
    /// </summary>
    public static int GetCoinNeedToUpgradeArmor(int level)
    {
        if (level <= 1) return 0;
        int l = level - 1;
        return 1000 * l + 400 * l * l;
    }

    /// <summary>
    /// Tính ArmorPrimorite tích lũy cần để nâng cấp armor lên level chỉ định (1 -> 15).
    /// Common: ~770, Epic: ~1,925, Legendary: ~3,080 Primorite / món.
    /// </summary>
    public static int GetPrimoriteNeedToUpgradeArmor(Rare rare, int level)
    {
        if (level <= 1) return 0;
        int l = level - 1;
        float basePrimorite = 20f * l + 2.5f * l * l;

        float rareMultiplier = rare switch
        {
            Rare.Common => 1.0f,
            Rare.Uncommon => 1.3f,
            Rare.Rare => 1.8f,
            Rare.Epic => 2.5f,
            Rare.Legendary => 4.0f,
            _ => 1.0f
        };

        return Mathf.RoundToInt(basePrimorite * rareMultiplier);
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
    /// Hoàn trả: Giá trị phôi gốc + 80% số Primorite đã đầu tư nâng cấp.
    /// </summary>
    public static int GetArmorPrimoriteFromSalvage(Rare rare, int level)
    {
        int baseValue = rare switch
        {
            Rare.Common => 10,
            Rare.Uncommon => 25,
            Rare.Rare => 60,
            Rare.Epic => 150,
            Rare.Legendary => 400,
            _ => 10
        };

        if (level <= 1) return baseValue;

        int totalInvested = GetPrimoriteNeedToUpgradeArmor(rare, level);
        return baseValue + Mathf.RoundToInt(totalInvested * 0.8f);
    }

    /// <summary>
    /// Tính RelicEssence nhận được khi quy đổi (salvage/recall) vũ khí.
    /// Hoàn trả: Giá trị phôi gốc (Lv 1) theo Rare + 70% số RelicEssence đã đầu tư nâng cấp.
    /// </summary>
    public static int GetWeaponSalvageEssence(Rare rare, int level)
    {
        int baseValue = rare switch
        {
            Rare.Common => 10,
            Rare.Uncommon => 25,
            Rare.Rare => 60,
            Rare.Epic => 150,
            Rare.Legendary => 400,
            _ => 10
        };

        if (level <= 1) return baseValue;

        int totalInvested = GetEssenceNeedToUpgradeWeapon(level);
        return baseValue + Mathf.RoundToInt(totalInvested * 0.7f);
    }

    /// <summary>
    /// Hệ số nhân chỉ số Main Stat theo phẩm chất (Rarity Multiplier).
    /// Common: x1.0, Uncommon: x1.25, Rare: x1.6, Epic: x2.2, Legendary: x3.0
    /// </summary>
    public static float GetRarityMultiplier(Rare rare)
    {
        return rare switch
        {
            Rare.Common => 1.0f,
            Rare.Uncommon => 1.25f,
            Rare.Rare => 1.6f,
            Rare.Epic => 2.2f,
            Rare.Legendary => 3.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Giới hạn Level tối đa của trang bị theo độ hiếm.
    /// Common: 6, Uncommon: 9, Rare: 12, Epic: 15, Legendary: 15
    /// </summary>
    public static int GetMaxArmorLevelByRare(Rare rare)
    {
        return rare switch
        {
            Rare.Common => 6,
            Rare.Uncommon => 9,
            Rare.Rare => 12,
            Rare.Epic => 15,
            Rare.Legendary => 15,
            _ => 15
        };
    }

    /// <summary>
    /// Tính main stat của armor theo level và độ hiếm (Rare).
    /// MainStat = (baseValue × rarityMultiplier) × (1 + 0.12 × level)
    /// </summary>
    public static int GetArmorMainStatByLevel(float baseValue, int level, Rare rare = Rare.Common)
    {
        float rarityMult = GetRarityMultiplier(rare);
        float finalBase = baseValue * rarityMult;
        return Convert.ToInt32(finalBase + finalBase * 0.12f * level);
    }

    /// <summary>
    /// Quy định số lượng dòng substat ban đầu theo độ hiếm (Rare) của món giáp.
    /// Common: 0, Uncommon: 1, Rare: 2, Epic: 3, Legendary: 4
    /// </summary>
    public static int GetInitialSubstatCountByRare(Rare rare)
    {
        return rare switch
        {
            Rare.Common => 0,
            Rare.Uncommon => 1,
            Rare.Rare => 2,
            Rare.Epic => 3,
            Rare.Legendary => 4,
            _ => 0
        };
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

