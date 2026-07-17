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

#if UNITY_EDITOR
        // HACK: Sửa lỗi Addressables Play Mode (Use Asset Database) ném ra ngoại lệ InvalidKeyException 
        // với SpriteAtlasV2 (bị nhận diện nhầm là DefaultAsset).
        string[] guids = UnityEditor.AssetDatabase.FindAssets(address + " t:spriteatlas");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas != null)
            {
                loadedAtlases[address] = atlas;
                return atlas;
            }
        }
#endif

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