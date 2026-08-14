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

    // Cache lại các Field của LocKeys để tăng hiệu suất Reflection
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
            // Load file text từ AddressablesManager
            TextAsset textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(addressKey);

            if (textAsset == null)
            {
                Debug.LogError($"Don't find localization file: {addressKey}");
                return;
            }

            localization = Json.DeserializeObject<Dictionary<long, string>>(textAsset.text);

            if (localization == null)
            {
                localization = new Dictionary<long, string>();
                Debug.LogError("Failed to parse localization JSON.");
            }

            isReady = true;
            Debug.Log($"Loaded localization: {languageCode} ({localization.Count} entries)");

            // Giải phóng asset khỏi cache AddressablesManager an toàn
            AddressablesManager.Instance.RemoveAsset(addressKey);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed when load localization {languageCode}: {e.Message}");
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

        if (hashKey == 0)
            return "";
        return missingTextString;
    }

    // Get content by string id
    public string GetLocalizedValue(string stringID)
    {
        if (string.IsNullOrEmpty(stringID)) return "";

        string cleanID = stringID.Trim().ToUpper();

        // check in cache before Reflection 
        if (!keyCache.TryGetValue(cleanID, out long hashKey))
        {
            // use Reflection to get LocKeys
            FieldInfo field = typeof(LocKeys).GetField(cleanID, BindingFlags.Public | BindingFlags.Static);

            if (field != null && field.IsLiteral) // IsLiteral make sure is constant
            {
                hashKey = (long)field.GetValue(null);
                keyCache[cleanID] = hashKey; // cache data
            }
            else
            {
                Debug.LogWarning($"Key UUID '{stringID}' (cleaned: '{cleanID}') not found in LocKeys class.");
                return stringID;
            }
        }

        return GetLocalizedValue(hashKey);
    }

    /// <summary>
    /// Lấy chuỗi dịch theo templateKey và tự động thay thế biến trong ngoặc {paramName}
    /// Ví dụ: GetLocalizedFormat("msg_not_enough_resource", ("resource_name", "Vàng")) -> "Không đủ Vàng!"
    /// </summary>
    public string GetLocalizedFormat(string templateKey, params (string paramName, string paramValue)[] args)
    {
        string text = GetLocalizedValue(templateKey);
        if (string.IsNullOrEmpty(text) || text == missingTextString) return templateKey;

        if (args != null)
        {
            foreach (var (paramName, paramValue) in args)
            {
                text = text.Replace("{" + paramName + "}", paramValue);
            }
        }
        return text;
    }

    /// <summary>
    /// Overload đơn giản với 1 tham số thay thế {paramName}
    /// </summary>
    public string GetLocalizedFormat(string templateKey, string paramName, string paramValue)
    {
        return GetLocalizedFormat(templateKey, (paramName, paramValue));
    }

    public bool IsReady => isReady;
}
