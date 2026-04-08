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
    private Dictionary<string, EffectConfig> EffectConfigs = new Dictionary<string, EffectConfig>();
    private Dictionary<string, AscensionConfig> AscensionConfigs  = new Dictionary<string, AscensionConfig>();
    private Dictionary<string, ExpConfig> ExpConfigs = new Dictionary<string, ExpConfig>();
    private Dictionary<string, StarUpConfig> StarUpConfigs = new Dictionary<string, StarUpConfig>();
    private Dictionary<string, SetBonusConfig> SetBonusConfigs = new Dictionary<string, SetBonusConfig>();

    private const string ItemConfigsAddress = "ItemsConfig";

    private const string CharacterConfigsAddress = "CharacterConfig";

    private const string BattleConfigAddress = "BattleConfig";

    private const string EffectConfigAddress = "EffectConfig";

    private const string AscensionAddress = "AscensionConfig";

    private const string ExpConfigAddress = "ExpConfig";

    private const string StarUpConfigAddress = "StarUpConfig";

    private const string SetBousConfigAdress = "SetBonusConfig";
    public async UniTask Init(CancellationToken cancellationToken = default)
    {
        // 1. Load JSON
        var (itemText, characterText, battleText, effectText) = await UniTask.WhenAll(
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(ItemConfigsAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(CharacterConfigsAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(BattleConfigAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(EffectConfigAddress, token: cancellationToken)
        );

        var (expText, ascensionText, starUpText, setBonusText) = await UniTask.WhenAll(
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(ExpConfigAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(AscensionAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(StarUpConfigAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(SetBousConfigAdress, token: cancellationToken)
        );

        ItemConfigs = Json.DeserializeObject<Dictionary<string, ItemConfig>>(itemText.text);
        CharacterConfigs = Json.DeserializeObject<Dictionary<string, CharacterConfig>>(characterText.text);
        BattleConfigs = Json.DeserializeObject<Dictionary<string, BattleConfig>>(battleText.text);
        EffectConfigs =  Json.DeserializeObject<Dictionary<string,EffectConfig>>(effectText.text);

        ExpConfigs = Json.DeserializeObject<Dictionary<string, ExpConfig>>(expText.text);
        AscensionConfigs = Json.DeserializeObject<Dictionary<string, AscensionConfig>>(ascensionText.text);
        StarUpConfigs = Json.DeserializeObject<Dictionary<string, StarUpConfig>>(starUpText.text);
        SetBonusConfigs = Json.DeserializeObject<Dictionary<string, SetBonusConfig>>(setBonusText.text);

        AddressablesManager.Instance.RemoveAsset(ItemConfigsAddress);
        AddressablesManager.Instance.RemoveAsset(CharacterConfigsAddress);
        AddressablesManager.Instance.RemoveAsset(BattleConfigAddress);
        AddressablesManager.Instance.RemoveAsset(EffectConfigAddress);

        AddressablesManager.Instance.RemoveAsset(ExpConfigAddress);
        AddressablesManager.Instance.RemoveAsset(AscensionAddress);
        AddressablesManager.Instance.RemoveAsset(StarUpConfigAddress);
        AddressablesManager.Instance.RemoveAsset(SetBousConfigAdress);

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
        atlasAddresses.Add("Atlas_icon_character_rare");


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
            config.BaseSkillIcon = atlasProvider.GetSprite("Atlas_skill_ui", charItem.Key + "_Attack");
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

    public string GetCharacterRareID(CharacterRare rare)
    {
        switch (rare)
        {
            case CharacterRare.R:
                return "R_Icon";
            case CharacterRare.SR:
                return "SR_Icon";
            case CharacterRare.SSR:
                return "SSR_Icon";
            case CharacterRare.UR:
                return "UR_Icon";
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
    public Sprite GetCharacterRareIcon(CharacterRare rare)
    {
        string spriteID = GetCharacterRareID(rare);

        return atlasProvider.GetSprite("Atlas_icon_character_rare", spriteID);
    }

    public EffectConfig GetEffectConfig(string key)
    {
        EffectConfigs.TryGetValue(key, out EffectConfig effectConfig);
        return effectConfig;
    }

    public ExpConfig GetExpConfig(string key)
    {
        ExpConfigs.TryGetValue(key, out ExpConfig expConfig);
        return expConfig;
    }

    public AscensionConfig GetAscensionConfig(string key)
    {
        AscensionConfigs.TryGetValue(key, out AscensionConfig ascensionConfig);
        return ascensionConfig;
    }

    public StarUpConfig GetStarUpConfig(string key)
    {
        StarUpConfigs.TryGetValue(key, out StarUpConfig starUpConfig);
        return starUpConfig;
    }

    public SetBonusConfig GetSetBonusConfig(string key)
    {
        SetBonusConfigs.TryGetValue(key, out SetBonusConfig setBonusConfig);

        return setBonusConfig;
    }

}



