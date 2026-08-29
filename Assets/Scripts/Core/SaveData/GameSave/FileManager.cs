using System;
using System.IO;
using UnityEngine;
using Tech.Json;

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

        // Nếu file save chưa tồn tại trên máy, khởi tạo từ file mẫu trong Resources/Data/
        if (!File.Exists(fullPath))
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            TextAsset textAsset = Resources.Load<TextAsset>($"Data/{nameWithoutExt}")
                               ?? Resources.Load<TextAsset>(nameWithoutExt)
                               ?? Resources.Load<TextAsset>($"Data/{nameWithoutExt.ToLowerInvariant()}")
                               ?? Resources.Load<TextAsset>(nameWithoutExt.ToLowerInvariant());

            if (textAsset != null)
            {
                File.WriteAllText(fullPath, textAsset.text);
            }
            else
            {
                Debug.LogWarning($"[FileManager] Could not find default template for {fileName} in Resources.");
            }
        }

        try
        {
            if (File.Exists(fullPath))
            {
                Json.LoadJson(fullPath, out result);

                // Tự động khôi phục nếu file save trên thiết bị bị rỗng (do lỗi ở phiên bản cũ)
                if (result is PlayerSave playerSave)
                {
                    if (playerSave.Roster == null || playerSave.Roster.Characters == null || playerSave.Roster.Characters.Count == 0)
                    {
                        Debug.LogWarning($"[FileManager] {fullPath} contains empty player roster. Restoring from default template in Resources.");
                        var defaultAsset = Resources.Load<TextAsset>("Data/player") 
                                        ?? Resources.Load<TextAsset>("player");
                        if (defaultAsset != null)
                        {
                            File.WriteAllText(fullPath, defaultAsset.text);
                            Json.LoadJson(fullPath, out result);
                        }
                    }
                }

                if (result != null) return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read file from {fullPath} with error {e}");
        }

        result = default;
        return false;
    }
}
