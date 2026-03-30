using System.Collections;
using System.Collections.Generic;
using VContainer;
using System;
public class InventoryManager 
{
    [Inject] GameDataBase _gameDataBase;
    [Inject] SaveSystem _save;

    public event Action OnInventoryChanged;

    private InventorySaveData _saveData => _save.Player.Inventory;

    public IReadOnlyList<WeaponSaveData> Weapons => _saveData.Weapons;
    public IReadOnlyList<ArmorSaveData> Armors => _saveData.Armors;
    public IReadOnlyList<ItemSaveData> Items => _saveData.Items;

    public void AddStackableItem(string itemID, ItemType type, int amount)
    {
        if(amount < 0) amount = 0;

        var exitstringItem = _save.Player.Inventory.GetItem(itemID);

        if(exitstringItem != null) 
        {
            exitstringItem.Quantity += amount;
        }
        else
        {
            _save.Player.Inventory.Items.Add(new ItemSaveData {
                ID = itemID,
                Type = type,
                Quantity = amount
            });
        }

        OnInventoryChanged?.Invoke();
    }


    // Trả về true nếu trừ thành công, false nếu không đủ đồ.
    public bool ConsumeStackableItem(string itemID, int amount)
    {
        if(amount <= 0 ) return true;

        var existingItem  = _save.Player.Inventory.GetItem(itemID);

        if(existingItem == null || existingItem.Quantity < amount)
        {
            return false;
        }

        existingItem.Quantity -= amount;

        if(existingItem.Quantity <= 0)
        {
            _save.Player.Inventory.Items.Remove(existingItem);
        }

        OnInventoryChanged?.Invoke();

        return true;
    }

    public int GetITemQuantity(string itemID)
    {
        var existingItem = _save.Player.Inventory.Items.Find(item => item.ID == itemID);
        return existingItem != null ? existingItem.Quantity : 0;
    }

    public void AddWeapon(WeaponSaveData newWeapon)
    {
        if(newWeapon == null) return;
        _save.Player.Inventory.Weapons.Add(newWeapon);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveWeapon(string uuid)
    {
        var weaponToRemove = _save.Player.Inventory.GetWeapon(uuid);

        if(weaponToRemove != null )
        {
            _save.Player.Inventory.Weapons.Remove(weaponToRemove);
            OnInventoryChanged?.Invoke();
        }
    }

    public WeaponSaveData GetWeapon(string uuid)
    {
        return _save.Player.Inventory.GetWeapon(uuid);
    }

    public void AddArmor(ArmorSaveData newArmor)
    {
        if(newArmor == null) return;
        _save.Player.Inventory.Armors.Add(newArmor);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveArmor(string uuid)
    {
        var armorToRemove = _save.Player.Inventory.GetArmor(uuid);

        if(armorToRemove != null)
        {
            _save.Player.Inventory.Armors.Remove(armorToRemove);
            OnInventoryChanged?.Invoke();
        }
    }

    public ArmorSaveData GetArmor(string uuid)
    {
        return _save.Player.Inventory.GetArmor(uuid);
    }
}
