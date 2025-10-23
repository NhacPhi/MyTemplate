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

    private List<WeaponConfig> weaponConfig;
    private List<FoodConfig> foodConfig;
    public List<ItemBaseSO> Avatars { get { return avatars; } }

    private Dictionary<ItemType, List<ItemBaseSO>> dictionaryItemSO = new();
    private void Start()
    {
        LoadItemDataConfig();
        dictionaryItemSO.Add(ItemType.Weapon, weapons);
        dictionaryItemSO.Add(ItemType.Food, foods);
        dictionaryItemSO.Add(ItemType.Avatar, avatars);
    }

    private void LoadItemDataConfig()
    {
        string path = "Assets/Data/GameConfig/";

        string fileWeapon = "Weapon.json";
        string fileFood = "Food.json";

        Json.LoadJson(Path.Combine(path, fileWeapon), out weaponConfig);
        Json.LoadJson(Path.Combine(path, fileFood), out foodConfig);
    }

    public T GetItemConfigByID<T>(ItemType type, string id) where T : ItemBaseConfig
    {
        switch(type)
        {
            case ItemType.Weapon:
                return weaponConfig.Find(weapon => weapon.ID == id) as T;
            case ItemType.Food:
                return foodConfig.Find(weapon => weapon.ID == id) as T;
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
