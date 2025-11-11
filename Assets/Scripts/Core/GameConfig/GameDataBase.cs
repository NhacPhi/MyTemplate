using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
public class GameDataBase : MonoBehaviour
{
    [SerializeField] private List<ItemBaseSO> avatars;
    [SerializeField] private List<RareSO> rares;
    [SerializeField] private List<ItemBaseSO> weapons;
    [SerializeField] private List<ItemBaseSO> foods;
    [SerializeField] private List<ItemBaseSO> gemStones;
    [SerializeField] private List<ItemBaseSO> armors;
    [SerializeField] private List<ItemBaseSO> shards;
    [SerializeField] private List<ItemBaseSO> exps;
    [SerializeField] private List<CharacterRareSO> characterRares;

    [SerializeField] private List<CharacterSO> characters;

    private List<WeaponConfig> weaponConfig;
    private List<FoodConfig> foodConfig;
    private List<GemStoneConfig> gemStoneConfig;
    private List<BaseArmorConfig> baseArmorConfig;
    private List<ShardConfig> shardConfig;
    private List<CharacterConfig> characterConfig;
    private List<CharacterStatConfig> characterStatConfig;
    private List<CharacterUpgradeConfig> characterUpgradeConfig;
    private List<ExpConfig> expConfig;
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
        dictionaryItemSO.Add(ItemType.Shard, shards);
        dictionaryItemSO.Add(ItemType.Exp, exps);
    }

    private void LoadItemDataConfig()
    {
        string path = "Assets/Data/GameConfig/";

        string fileWeapon = "Weapon.json";
        string fileFood = "Food.json";
        string fileGemStone = "GemStone.json";
        string fileBaseArmor = "BaseArmor.json";
        string fileShardConfig = "Shard.json";
        string fileCharacterConfig = "Character.json";
        string fileCharacterStat = "CharacterStat.json";
        string fileCharacterUpgrade = "CharacterUpgrade.json";
        string fileExpConfig = "Exp.json";

        Json.LoadJson(Path.Combine(path, fileWeapon), out weaponConfig);
        Json.LoadJson(Path.Combine(path, fileFood), out foodConfig);
        Json.LoadJson(Path.Combine(path, fileGemStone), out gemStoneConfig);
        Json.LoadJson(Path.Combine(path, fileBaseArmor), out baseArmorConfig);
        Json.LoadJson(Path.Combine(path, fileShardConfig), out shardConfig);
        Json.LoadJson(Path.Combine(path, fileCharacterConfig), out characterConfig);
        Json.LoadJson(Path.Combine(path, fileCharacterStat), out characterStatConfig);
        Json.LoadJson(Path.Combine(path, fileCharacterUpgrade), out characterUpgradeConfig);
        Json.LoadJson(Path.Combine(path, fileExpConfig), out expConfig);
    }

    public List<ExpConfig> ExpConfig => expConfig;

    public T GetItemConfigByID<T>(ItemType type, string id) where T : ItemBaseConfig
    {
        switch (type)
        {
            case ItemType.Weapon:
                return weaponConfig.Find(weapon => weapon.ID == id) as T;
            case ItemType.Food:
                return foodConfig.Find(food => food.ID == id) as T;
            case ItemType.GemStone:
                return gemStoneConfig.Find(gemstone => gemstone.ID == id) as T;
            case ItemType.Armor:
                return baseArmorConfig.Find(armor => armor.ID == id) as T;
            case ItemType.Shard:
                return shardConfig.Find(shard => shard.ID == id) as T;
            case ItemType.Exp:
                return expConfig.Find(exp => exp.ID == id) as T;
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

    public CharacterSO GetCharacterSO(string id)
    {
        return characters.Find(character => character.ID == id);
    }

    public CharacterRareSO GetCharacterRareSO(CharacterRare rare)
    {
        return characterRares.Find(v => v.Rare == rare);
    }

    public CharacterConfig GetCharacterConfig(string id)
    {
        return characterConfig.Find(characterConfig=> characterConfig.ID == id);
    }

    public CharacterStatConfig GetCharacterStatConfig(string id)
    {
        return characterStatConfig.Find(charcter => charcter.ID == id);
    }
    public CharacterUpgradeConfig GetCharacterUpgradeConfig(string id)
    {
        return characterUpgradeConfig.Find(charcter => charcter.ID == id);
    }

    public WeaponConfig GetWeaponConfig(string id)
    {
        return weaponConfig.Find(weapon => weapon.ID == id);
    }
    public Sprite GetRareBG(Rare type)
    {
        return rares.Find(v => v.Type == type).Image;
    }
}
