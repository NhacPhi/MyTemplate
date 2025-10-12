using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using Tech.Singleton;

public class LocalizationManager : SingletonPersistent<LocalizationManager>
{
    private Dictionary<string, string> _localization = new Dictionary<string, string>();
    private bool _isReady = false;
    private string _missingTextString = "Localized text not found";

    private void Awake()
    {
        base.Awake();
    }
    public async UniTask LoadLocalizedText(string languageCode)
    {
        _localization.Clear();

        _isReady = false;

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

                _localization[key] = value;
            }

            _isReady = true;
            Debug.Log($"Loaded localization: {languageCode} ({_localization.Count} entries)");

            // Release resource (Addressables hold reference)
            Addressables.Release(textAsset);
        }
        catch (Exception e) {
            Debug.LogError($"Faile when load localization {languageCode}: {e.Message}");
        }

    }

    public string GetLocalizedValue(string key)
    {
        if (!_isReady || !_localization.ContainsKey(key)) return _missingTextString;
        return _localization[key].Replace("\\n", "\n");
    }

    public bool IsReady => _isReady;
}
