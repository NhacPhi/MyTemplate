using System.Collections;
using System.Collections.Generic;
using VContainer;

public enum CurrencyType
{
    Energy,
    Jade,
    Coin,
    RelicEssence,
    ArmorPrimorite,
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
        if (currencies == null) currencies = new Dictionary<CurrencyType, int>();
        if (!currencies.ContainsKey(type)) currencies[type] = 0;

        currencies[type] += amount;
        Save();
        UIEvent.OnCurrencyChanged?.Invoke(type, currencies[type]);
    }

    public bool Spend(CurrencyType type, int amount)
    {
        if (currencies == null) currencies = new Dictionary<CurrencyType, int>();
        if (!currencies.ContainsKey(type)) currencies[type] = 0;

        if (currencies[type] < amount) return false;
        
        currencies[type] -= amount;
        Save();

        UIEvent.OnCurrencyChanged?.Invoke(type, currencies[type]); 
        return true;
    }

    private void Save()
    {
        save.Player.Inventory.SetCurrency(currencies);
        save.SaveDataToDisk(GameSaveType.PlayerInfo);
    }

    private void Load()
    {
        currencies = save.Player.Inventory.Currencies;
        if (currencies == null)
        {
            currencies = new Dictionary<CurrencyType, int>();
            save.Player.Inventory.Currencies = currencies;
        }
    }
    public void UpdateCurrency()
    {
        foreach (var currency in currencies)
        {
            UIEvent.OnCurrencyChanged?.Invoke(currency.Key, currency.Value);
        }
    }

    public int GetQuantityCurrecy(CurrencyType type)
    {
        if (currencies == null) return 0;
        int result;
        currencies.TryGetValue(type, out result);
        return result;
    }
}
