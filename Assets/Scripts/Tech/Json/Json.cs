using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Converters;

namespace Tech.Json
{
    public static class Json
    {
        private static readonly string _key = "gCjK+DZ/GCYbKIGiAt1qCA==";
        private static readonly string _iv = "47l5QsSe1POo31adQ/u7nQ==";

        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        public static IEncryption Encryption = new AES(_key, _iv);
        //public static IEncryption Encryption;

        public static void SaveJson<T>(this T data, string path, bool useEncryption = false)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, settings);
                WriteAllText(path, json, useEncryption);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            catch { 
                //Ignored
            }
        }

        public static async void SaveJsonAsync<T>(this T data, string path, Action saveDone = null, bool useEncryption = false)
        {
            try
            {
                await UniTask.RunOnThreadPool(async () =>
                {
                    string json = JsonConvert.SerializeObject(data, settings);
                    await WriteAllTextAsync(path, Encryption.Encrypt(json), useEncryption);
                });

                saveDone?.Invoke();
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            catch { 
                // Ignored
            }
        }

        public static void LoadJson<T>(string path, out T value, bool useEncryption = false)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = ReadAllText(path, useEncryption);
                    T data = JsonConvert.DeserializeObject<T>(json, settings);
                    value = data;
                    return;
                }
            }
            catch
            {
                // Ignored
            }


            value = default;
        }

        private static async UniTask WriteAllTextAsync(string path, string text, bool useEncryption = false)
        {
            if(Encryption != null && useEncryption)
            {
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(Encryption.Encrypt(text), settings));
                return;
            }

            await File.WriteAllTextAsync(path, text);
        }

        private static void WriteAllText(string path,string text, bool useEncryption = false)
        {
            if(Encryption != null && useEncryption)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Encryption.Encrypt(text), settings));
                return;
            }

            File.WriteAllText(path, text);
        }

        public static string ReadAllText(string path, bool useEncryption = false)
        {
            if(Encryption != null && useEncryption)
            {
                return Encryption.Decrypt(JsonConvert.DeserializeObject<string>(File.ReadAllText(path), settings));
            }

            return File.ReadAllText(path);
        }

        public static T DeserializeObject<T>(string json, bool useEncryption = false)
        {
            var newJson = json;

            if(Encryption != null && useEncryption)
            {
                newJson = Encryption.Decrypt(json);
            }
            return JsonConvert.DeserializeObject<T>(newJson, settings);
        }

        public static string SerializeObject<T>(T data, bool useEncryption = false)
        {
            var json = JsonConvert.SerializeObject(data, settings);

            if (Encryption != null && useEncryption)
            {
                json = Encryption.Encrypt(json);
            }

            return json;
        }
    }
}
