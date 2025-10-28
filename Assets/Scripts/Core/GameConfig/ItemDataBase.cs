using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
public class ItemDataBase : MonoBehaviour
{
    [SerializeField] private List<ItemBaseSO> avatars;
    [SerializeField] private List<RareSO> rares;
    [SerializeField] private List<ItemBaseSO> weapons;
    [SerializeField] private List<ItemBaseSO> foods;
    [SerializeField] private List<ItemBaseSO> gemStones;
    [SerializeField] private List<ItemBaseSO> armors;

    private List<WeaponConfig> weaponConfig;
    private List<FoodConfig> foodConfig;
    private List<GemStoneConfig> gemStoneConfig;
    private List<BaseArmorConfig> baseArmorConfig;
    public List<ItemBaseSO> Avatars { get { return avatars; } }

    private Dictionary<ItemType, List<ItemBaseSO>> dictionaryItemSO = new();
    private void Start()
    {
        LoadItemDataConfig();
        dictionaryItemSO.Add(ItemType.Weapon, weapons);
        dictionaryItemSO.Add(ItemType.Food, foods);
        dictionaryItemSO.Add(ItemType.Avatar, avatars);
        dictionaryItemSO.Add(ItemType.GemStone, gemStones);
        dictionaryItemSO.Add(ItemType.Armor, armors);
    }

    private void LoadItemDataConfig()
    {
        string path = "Assets/Data/GameConfig/";

        string fileWeapon = "Weapon.json";
        string fileFood = "Food.json";
        string fileGemStone = "GemStone.json";
        string fileBaseArmor = "BaseArmor.json";

        Json.LoadJson(Path.Combine(path, fileWeapon), out weaponConfig);
        Json.LoadJson(Path.Combine(path, fileFood), out foodConfig);
        Json.LoadJson(Path.Combine(path, fileGemStone), out gemStoneConfig);
        Json.LoadJson(Path.Combine(path, fileBaseArmor), out baseArmorConfig);
    }

    public T GetItemConfigByID<T>(ItemType type, string id) where T : ItemBaseConfig
    {
        switch(type)
        {
            case ItemType.Weapon:
                return weaponConfig.Find(weapon => weapon.ID == id) as T;
            case ItemType.Food:
                return foodConfig.Find(food => food.ID == id) as T;
            case ItemType.GemStone:
                return gemStoneConfig.Find(gemstone => gemstone.ID == id) as T;
            case ItemType.Armor:
                return baseArmorConfig.Find(armor => armor.ID == id) as T;
        }
        return null;
    }

    public T GetItemSOByID<T>(ItemType type, string id) where T : ItemBaseSO
    {
        if (dictionaryItemSO.TryGetValue(type, out var list))
        {
            return list.Find(item => item.ID == id) as T;
        }
        return null;
    }

    public Sprite GetRareBG(Rare type)
    {
        return rares.Find(v => v.Type == type).Image;
    }
}
