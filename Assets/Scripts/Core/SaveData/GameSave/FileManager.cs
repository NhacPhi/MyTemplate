using System;
using System.IO;
using UnityEngine;
using Tech.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class FileManager
{
    public static bool WriteToFile<T>(string fileName, T content)
    {
        string fullPath;
#if UNITY_EDITOR
        fullPath = Path.Combine("Assets/Data", fileName);
#elif UNITY_ANDROID
        fullPath = Path.Combine(Application.persistentDataPath, fileName);
#else
        fullPath = Path.Combine("Assets/Data", fileName);
#endif


        try
        {
            Json.SaveJson(content, fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFromFile<T>(string fileName, out T result)
    {
        string fullPath;
#if UNITY_EDITOR
        fullPath = Path.Combine("Assets/Data", fileName);
#elif UNITY_ANDROID
        fullPath = Path.Combine(Application.persistentDataPath, fileName);
#else
        fullPath = Path.Combine("Assets/Data", fileName);
#endif
        if (!File.Exists(fullPath))
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(fileName);

            var textAsset = handle.WaitForCompletion();


            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                File.WriteAllText(fullPath, textAsset.text);
            }
            Addressables.Release(handle);
        }

        try
        {
            Json.LoadJson(fullPath, out result);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read file from {fullPath} with error {e}");
            result = default;
            return false;
        }
    }
}
