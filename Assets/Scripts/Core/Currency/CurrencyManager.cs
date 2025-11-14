using System.Collections;
using System.Collections.Generic;
using VContainer;

public enum CurrencyType
{
    Energy,
    Gem,
    Coin,
    Essence,
    Ticket
}
public class CurrencyManager
{
    [Inject] private SaveSystem save;

    private Dictionary<CurrencyType, int> currencies = new();
    public Dictionary<CurrencyType, int> Currencies => currencies;
    public int GetValue(CurrencyType type) => currencies.TryGetValue(type, out var val) ? val : 0;

    public void Init()
    {
        Load();
    }
    public void Add(CurrencyType type, int amount)
    {
        currencies[type] += amount;
        Save();
        UIEvent.OnCurrencyChanged(type, currencies[type]);
    }

    public bool Spend(CurrencyType type, int amount)
    {
        if(GetValue(type) < amount) return false;
        Save();
        currencies[type] -= amount;

        UIEvent.OnCurrencyChanged(type, amount); 
        return true;
    }

    private void Save()
    {
        save.Player.Currencies = currencies;
        save.SaveDataToDisk(GameSaveType.PlayerInfo);
    }

    private void Load()
    {
        currencies = save.Player.Currencies;
    }
    public void UpdateCurrency()
    {
        foreach (var currency in currencies)
        {
            UIEvent.OnCurrencyChanged?.Invoke(currency.Key, currency.Value);
        }
    }
}
