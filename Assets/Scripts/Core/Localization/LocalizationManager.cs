using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using Tech.Singleton;
using Tech.Json;
using System.Reflection;

public class LocalizationManager : SingletonPersistent<LocalizationManager>
{
    private Dictionary<long, string> localization = new Dictionary<long, string>();
    private bool isReady = false;
    private string missingTextString = "Localized text not found";

    // Cache l?i các Field c?a LocKeys ?? t?ng hi?u su?t Reflection
    private Dictionary<string, long> keyCache = new Dictionary<string, long>();

    private void Awake()
    {
        base.Awake();
    }
    public async UniTask LoadLocalizedText(string languageCode)
    {
        localization.Clear();

        isReady = false;

        string addressKey = $"Localization_{languageCode}";

        try
        {
            //Load file text from Addressable
            TextAsset textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(addressKey);

            if(textAsset == null)
            {
                Debug.LogError($"Don't find localization file: {addressKey}");
            }

            //string[] lines = textAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            localization = Json.DeserializeObject<Dictionary<long, string>>(textAsset.text);

            if (localization == null)
            {
                localization = new Dictionary<long, string>();
                Debug.LogError("Failed to parse localization JSON.");
            }

            isReady = true;
            Debug.Log($"Loaded localization: {languageCode} ({localization.Count} entries)");

            // Release resource (Addressables hold reference)
            Addressables.Release(textAsset);
        }
        catch (Exception e) {
            Debug.LogError($"Faile when load localization {languageCode}: {e.Message}");
        }

    }

    // Get content by uint id
    public string GetLocalizedValue(long hashKey)
    {
        if (!isReady) return "Loading...";

        if (localization.TryGetValue(hashKey, out string value))
        {
            return value.Replace("\\n", "\n");
        }

        return missingTextString;
    }

    // Get content by string id
    public string GetLocalizedValue(string stringID)
    {
        if (string.IsNullOrEmpty(stringID)) return "";

        // check in cache before Reflection 
        if (!keyCache.TryGetValue(stringID, out long hashKey))
        {
            //uuse Reflection to get Lockeys
            FieldInfo field = typeof(LocKeys).GetField(stringID, BindingFlags.Public | BindingFlags.Static);

            if (field != null && field.IsLiteral) // IsLiteral make sure is constant
            {
                hashKey = (long)field.GetValue(null);
                keyCache[stringID] = hashKey; // cache data
            }
            else
            {
                Debug.LogWarning($"Key ID '{stringID}' not found in LocKeys class.");
                return missingTextString;
            }
        }

        return GetLocalizedValue(hashKey);
    }

    public bool IsReady => isReady;
}
