using System.Collections;
using System.Collections.Generic;
using System;

public static class Utility 
{
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
}
