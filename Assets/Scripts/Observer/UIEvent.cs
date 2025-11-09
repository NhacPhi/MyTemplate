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

    //CharacterData
    public static Action<string> OnSelectCharacterAvatar;
    public static Action<CharacterTap> OnSelectToggleCharacterTap;

    public static Action<CurrencyType, int> OnCurrencyChanged;
}
