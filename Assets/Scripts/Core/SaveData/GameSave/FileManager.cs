using System;
using System.IO;
using UnityEngine;
using Tech.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class FileManager
{
    private static string GetFullPath(string fileName)
    {
        string path;
#if UNITY_EDITOR
        path = Path.Combine("Assets/Data", fileName);
#else
        path = Path.Combine(Application.persistentDataPath, fileName);
#endif
        EnsureDirectoryExists(path);
        return path;
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static bool WriteToFile<T>(string fileName, T content)
    {
        string fullPath = GetFullPath(fileName);

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
        string fullPath = GetFullPath(fileName);

        if (!File.Exists(fullPath))
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(fileName);

            var textAsset = handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Succeeded && textAsset != null)
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
