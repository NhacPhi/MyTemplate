using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.U2D;
using VContainer;
using System.Linq;

public class GameDataBase
{
    [Inject] AtlasProvider atlasProvider;

    private Dictionary<string, ItemConfig> ItemConfigs = new Dictionary<string, ItemConfig>();
    private Dictionary<string, CharacterConfig> CharacterConfigs = new Dictionary<string, CharacterConfig>();
    private Dictionary<string, BattleConfig> BattleConfigs = new Dictionary<string, BattleConfig>();

    private const string ItemConfigsAddress = "ItemsConfig";

    private const string CharacterConfigsAddress = "CharacterConfig";

    private const string BattleConfigAddress = "BattleConfig";
    public async UniTask Init(CancellationToken cancellationToken = default)
    {
        // 1. Load JSON
        var (itemText, characterText, battleText) = await UniTask.WhenAll(
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(ItemConfigsAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(CharacterConfigsAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(BattleConfigAddress, token: cancellationToken)
        );
        ItemConfigs = Json.DeserializeObject<Dictionary<string, ItemConfig>>(itemText.text);
        CharacterConfigs = Json.DeserializeObject<Dictionary<string, CharacterConfig>>(characterText.text);
        BattleConfigs = Json.DeserializeObject<Dictionary<string, BattleConfig>>(battleText.text);

        AddressablesManager.Instance.RemoveAsset(ItemConfigsAddress);
        AddressablesManager.Instance.RemoveAsset(CharacterConfigsAddress);
        AddressablesManager.Instance.RemoveAsset(BattleConfigAddress);

        // 2. Gom danh sách các Atlas cần dùng (để nạp 1 lần duy nhất)
        var atlasAddresses = new HashSet<string>();
        atlasAddresses.Add("Atlas_consumables");
        atlasAddresses.Add("Atlas_icon_armor");
        atlasAddresses.Add("Atlas_icon_gemstone");
        atlasAddresses.Add("Atlas_icon_weapon");
        atlasAddresses.Add("Atlas_big_icon_weapon");

        atlasAddresses.Add("Atlas_icon_game");
        atlasAddresses.Add("Atlas_icon_character");
        atlasAddresses.Add("Atlas_big_icon_character");
        atlasAddresses.Add("Atlas_image_character");
        atlasAddresses.Add("Atlas_skill_ui");


        //foreach (var item in ItemConfigs.Values) atlasAddresses.Add(item.AtlasAddress);

        var preloadTasks = atlasAddresses
            .Where(addr => !string.IsNullOrEmpty(addr))
            .Select(addr => atlasProvider.LoadAtlasAsync(addr));
        await UniTask.WhenAll(preloadTasks);

        // Load icon for all item
        foreach (var item in ItemConfigs)
        {
            var config = item.Value;
            config.Icon = atlasProvider.GetSprite(config.AtlasAddress, item.Key);
            config.IconBG = atlasProvider.GetSprite("Atlas_icon_game", GetRarityID(config.Rarity));

            if (config.Type == ItemType.Weapon && config.Weapon != null)
            {
                config.Weapon.BigIcon = atlasProvider.GetSprite("Atlas_big_icon_weapon", item.Key + "_big");
            }
        }

        foreach (var charItem in CharacterConfigs)
        {
            var config = charItem.Value;
            config.Icon = atlasProvider.GetSprite("Atlas_icon_character", charItem.Key);
            config.BigIcon = atlasProvider.GetSprite("Atlas_big_icon_character", charItem.Key + "_big");
            config.Image = atlasProvider.GetSprite("Atlas_image_character", "img_" + charItem.Key);
            config.AttackIcon = atlasProvider.GetSprite("Atlas_skill_ui", charItem.Key + "_Attack");
            config.MajorSkillIcon = atlasProvider.GetSprite("Atlas_skill_ui", charItem.Key + "_Major");
            config.UltimateSkillIcon = atlasProvider.GetSprite("Atlas_skill_ui", charItem.Key + "_Ultimate");
        }
    }

  

    private Dictionary<string, ItemConfig> avatarDict;

    public Dictionary<string, ItemConfig> AvatarDict
    {
        get
        {
            // cache
            if (avatarDict == null)
            {
                avatarDict = ItemConfigs
                    .Where(x => x.Value.Type == ItemType.Avatar)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            return avatarDict;
        }
    }

    public string GetRarityID(Rare type)
    {
        switch(type)
        {
            case Rare.Legendary:
                return "bg_lengendary";
            case Rare.Epic:
                return "bg_epic";
            case Rare.Rare:
                return "bg_rare";
            case Rare.Uncommon:
                return "bg_uncommon";
            case Rare.Common:
                return "bg_common";
        }

        return string.Empty;
    }

    public ItemConfig GetItemConfig(string key)
    {
        ItemConfigs.TryGetValue(key, out ItemConfig item);
        return item;
    }
    public CharacterConfig GetCharacterConfig(string key)
    {
        CharacterConfigs.TryGetValue(key, out CharacterConfig character);
        return character;
    }

    public BattleConfig GetBattleConfig(string key)
    {
        BattleConfigs.TryGetValue(key, out BattleConfig battleConfig);
        return battleConfig;
    }

    public Sprite GetBGItemByRare(Rare rare)
    {
        string spriteID = GetRarityID(rare);

        return atlasProvider.GetSprite("Atlas_icon_game", spriteID);
    }
}



