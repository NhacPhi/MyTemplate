using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine;

public class AtlasProvider
{
    private Dictionary<string, SpriteAtlas> loadedAtlases = new Dictionary<string, SpriteAtlas>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public async UniTask<SpriteAtlas> LoadAtlasAsync(string address)
    {
        if (loadedAtlases.TryGetValue(address, out var atlas)) return atlas;

        atlas = await AddressablesManager.Instance.LoadAssetAsync<SpriteAtlas>(address);
        loadedAtlases[address] = atlas;
        return atlas;
    }

    public Sprite GetSprite(string atlasAddress, string spriteName)
    {
        if (spriteCache.TryGetValue(spriteName, out var cachedSprite)) return cachedSprite;

        if (loadedAtlases.TryGetValue(atlasAddress, out var atlas))
        {
            var sprite = atlas.GetSprite(spriteName);
            if (sprite != null) spriteCache[spriteName] = sprite;
            return sprite;
        }
        return null;
    }
}