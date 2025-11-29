using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using Tech.Singleton;

public class LocalizationManager : SingletonPersistent<LocalizationManager>
{
    private Dictionary<string, string> localization = new Dictionary<string, string>();
    private bool isReady = false;
    private string missingTextString = "Localized text not found";

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
            TextAsset textAsset = await Addressables.LoadAssetAsync<TextAsset>(addressKey).Task;

            if(textAsset == null)
            {
                Debug.LogError($"Don't find localization file: {addressKey}");
            }

            string[] lines = textAsset.text.Split('\n');

            foreach(string line in lines )
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.Contains("=")) continue;

                int index = line.IndexOf('=');
                string key = line[..index].Trim();
                string value = line[(index + 1)..].Trim();

                localization[key] = value;
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

    public string GetLocalizedValue(string key)
    {
        if (key == null) return "";
        if (!isReady || !localization.ContainsKey(key)) return missingTextString;
        return localization[key].Replace("\\n", "\n");
    }

    public bool IsReady => isReady;
}
