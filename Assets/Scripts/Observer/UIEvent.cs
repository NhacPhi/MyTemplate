using System;

public static class UIEvent
{
    public static Action OnLanguageChanged;
    public static Action<string> OnClickNavigationButton;
    public static Action<string> OnChanageAvatarPopup;
    public static Action<string> OnChanageAvatarPanel;

    //Inventory
    public static Action<ItemType> OnSelectToggleInventoryTap;
    public static Action<string> OnSelectInventoryItem;

    //Character
    public static Action<string> OnSelectCharacterAvatar;
    public static Action<CharacterTap> OnSelectToggleCharacterTap;
    public static Action<string> OnSelectCharacterArmorUI;
    public static Action<string> OnSelectCharacterChangeWeapon;
    public static Action<string> OnSelectCharacterChangeArmor;

    //weapon
    public static Action<bool> OnCloseCharacterWeapon;
    public static Action<string> OnSelectWeaponCard;

    public static Action<bool> OnSlectectRelicTap;
    public static Action<WeaponTap> OnSelectToggleWeaponTap;
    public static Action<string> OnSlelectWeaponEnchance;

    public static Action<CurrencyType, int> OnCurrencyChanged;
}
