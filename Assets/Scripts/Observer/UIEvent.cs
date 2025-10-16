using System;

public static class UIEvent
{
    public static Action OnLanguageChanged;
    public static Action<string> OnClickNavigationButton;
    public static Action<string> OnChanageAvatarPopup;
    public static Action<string> OnChanageAvatarPanel;

    public static Action<CurrencyType, int> OnCurrencyChanged;
}
