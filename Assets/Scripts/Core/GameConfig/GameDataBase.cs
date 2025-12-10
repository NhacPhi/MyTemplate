using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;

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
    public void Start()
    {
        //LoadItemDataConfig();
        dictionaryItemSO.Add(ItemType.Weapon, weapons);
        dictionaryItemSO.Add(ItemType.Food, foods);
        dictionaryItemSO.Add(ItemType.Avatar, avatars);
        dictionaryItemSO.Add(ItemType.GemStone, gemStones);
        dictionaryItemSO.Add(ItemType.Armor, armors);
        dictionaryItemSO.Add(ItemType.Shard, shards);
        dictionaryItemSO.Add(ItemType.Exp, exps);
    }

    public async UniTask Init(CancellationToken cancellationToken = default)
    {
        string keyWeapon = "Weapon";
        string keyFood = "Food";
        string keyGemStone = "GemStone";
        string keyBaseArmor = "BaseArmor";
        string keyShardConfig = "Shard";
        string keyCharacterConfig = "Character";
        string keyCharacterStat = "CharacterStat";
        string keyCharacterUpgrade = "CharacterUpgrade";
        string keyExpConfig = "Exp";

        var tasks = new List<UniTask>()
        {
            LoadWeaponConfig(keyWeapon, cancellationToken),
            LoadFoodConfig(keyFood, cancellationToken),
            LoadGemStoneConfig(keyGemStone,cancellationToken),
            LoadBaseArmorConfig(keyBaseArmor, cancellationToken),
            LoadShardConfig(keyShardConfig, cancellationToken),
            LoadCharacterConfig(keyCharacterConfig, cancellationToken),
            LoadCharacterUpgradeConfig(keyCharacterUpgrade, cancellationToken),
            LoadCharacterStatConfig(keyCharacterStat, cancellationToken),
            LoadExpConfig(keyExpConfig, cancellationToken)
        };

        await UniTask.WhenAll(tasks);
    }

    public async UniTask LoadWeaponConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        weaponConfig = Json.DeserializeObject<List<WeaponConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadFoodConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        foodConfig = Json.DeserializeObject<List<FoodConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }

    public async UniTask LoadGemStoneConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        gemStoneConfig = Json.DeserializeObject<List<GemStoneConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }

    public async UniTask LoadBaseArmorConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        baseArmorConfig = Json.DeserializeObject<List<BaseArmorConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadShardConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        shardConfig = Json.DeserializeObject<List<ShardConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadCharacterConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        characterConfig = Json.DeserializeObject<List<CharacterConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadCharacterStatConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        characterStatConfig = Json.DeserializeObject<List<CharacterStatConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadCharacterUpgradeConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        characterUpgradeConfig = Json.DeserializeObject<List<CharacterUpgradeConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    public async UniTask LoadExpConfig(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        expConfig = Json.DeserializeObject<List<ExpConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
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
