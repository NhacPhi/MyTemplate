using System;
using System.IO;
using UnityEngine;
using Tech.Json;

public static class FileManager
{
    public static bool WriteToFile<T>(string fileName, T content)
    {
#if UNITY_ANDROID
        var fullPath = Path.Combine(Application.dataPath, fileName);
#else
        var fullPath = Path.Combine("Assets/Data", fileName);
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

#if UNITY_ANDROID
        var fullPath = Path.Combine(Application.dataPath, fileName);
#else
        var fullPath = Path.Combine("Assets/Data", fileName);
#endif
        if (!File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, "");
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
