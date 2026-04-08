using System.Collections;
using System.Collections.Generic;
using VContainer;
using System;
using NPOI.SS.Formula.Functions;

public class InventoryManager 
{
    [Inject] GameDataBase _gameDataBase;
    [Inject] SaveSystem _save;

    public event Action OnInventoryChanged;

    private InventorySaveData _saveData => _save.Player.Inventory;

    public List<WeaponSaveData> Weapons => _saveData.Weapons;
    public List<ArmorSaveData> Armors => _saveData.Armors;
    public List<ItemSaveData> Items => _saveData.Items;

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

    public ItemSaveData GetItem(string id)
    {
        return _save.Player.Inventory.GetItem(id);
    }

    public int GetItemQuantity(string itemID)
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

    public void SortAllByRare()
    {
        SortWeapons();
        SortArmors();
        SortItems();

        OnInventoryChanged?.Invoke();
    }

    private void SortWeapons()
    {
        if (Weapons == null) return;

        Weapons.Sort((a, b) =>
        {
            var cfgA = _gameDataBase.GetItemConfig(a.TemplateID);
            var cfgB = _gameDataBase.GetItemConfig(b.TemplateID);

            // 1. Ưu tiên sắp xếp theo Rarity trước (Giảm dần: B so với A)
            int rarityComparison = cfgB.Rarity.CompareTo(cfgA.Rarity);

            // Nếu Rarity khác nhau thì trả về luôn
            if (rarityComparison != 0)
            {
                return rarityComparison;
            }

            // 2. Nếu Rarity giống hệt nhau, tiếp tục sắp xếp theo Level (Giảm dần)
            return b.CurrentLevel.CompareTo(a.CurrentLevel);
        });
    }

    private void SortArmors()
    {
        if (Armors == null) return;

        Armors.Sort((a, b) =>
        {
            var cfgA = _gameDataBase.GetItemConfig(a.TemplateID);
            var cfgB = _gameDataBase.GetItemConfig(b.TemplateID);

            // 1. Ưu tiên sắp xếp theo Rarity trước (Giảm dần)
            int rarityComparison = cfgB.Rarity.CompareTo(cfgA.Rarity);

            if (rarityComparison != 0)
            {
                return rarityComparison;
            }

            // 2. Nếu Rarity giống nhau, sắp xếp theo Level (Giảm dần)
            // (Dựa theo code trước đó của bạn, Armor dùng .Level thay vì .CurrentLevel)
            return b.Level.CompareTo(a.Level);
        });
    }

    private void SortItems()
    {
        if (Items == null) return;

        Items.Sort((a, b) =>
        {
            var cfgA = _gameDataBase.GetItemConfig(a.ID);
            var cfgB = _gameDataBase.GetItemConfig(b.ID); ;

            return cfgB.Rarity.CompareTo(cfgA.Rarity);
        });
    }

    public List<ArmorSaveData> GetBestArmorsToEquip()
    {
        // 1. Chắc chắn danh sách đã xếp đồ xịn lên đầu
        SortArmors();

        List<ArmorSaveData> bestArmors = new List<ArmorSaveData>();

        // SỬ DỤNG TRỰC TIẾP ENUM ArmorPart ĐỂ ĐÁNH DẤU
        HashSet<ArmorPart> filledSlots = new HashSet<ArmorPart>();

        foreach (var armor in Armors)
        {
            if (!string.IsNullOrEmpty(armor.Equip))
            {
                continue;
            }

            var config = _gameDataBase.GetItemConfig(armor.TemplateID);

            // Lấy loại giáp từ config (Giả sử thuộc tính trong config tên là ArmorPart)
            ArmorPart currentArmorType = config.Armor.Part;

            // Nếu loại giáp này chưa có trong danh sách đồ xịn nhất
            if (!filledSlots.Contains(currentArmorType))
            {
                bestArmors.Add(armor);
                filledSlots.Add(currentArmorType);

                // Khi gom đủ 6 loại (Helmet, Chestplate, Gloves, Boots, Belt, Ring) thì dừng
                if (filledSlots.Count == 6)
                {
                    break;
                }
            }
        }

        return bestArmors;
    }

}
