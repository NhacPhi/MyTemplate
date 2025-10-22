using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
[System.Serializable]
public class AssetCustom
{
    private string name;
    private string path;
    public string PATH
    {
        get { return path; }
        set { path = value; }
    }
    public string Name 
    {
        get { return name; }
        set { name = value; }
    }
}

[System.Serializable]
public class DataSave
{
    private int fbs;
    private int musicVolune;
    private string currentLocalized;
    public int FPS
    {
        get { return fbs; }
        set { fbs = value; }
    }
    public int MusicVolune
    {
        get { return musicVolune; }
        set { musicVolune = value; }
    }

    public string CurrentLocalized
    {
        get { return currentLocalized; }
        set { currentLocalized = value; }
    }
}

public class Test : MonoBehaviour
{
    public const string SavePath = "Assets/Data/PlayerInfo.json";

    // Start is called before the first frame update
    void Start()
    {
        string path = "Assets/Prefabs/Cube.prefab";
        Debug.Log("GUID: " + path);
        AssetCustom asset = new AssetCustom();
        asset.Name = "Congyon";
        asset.PATH = path;

        string  fileName = "settings.json";
        string foldername = "Assets/Data";
        string pathFile = Path.Combine(foldername, fileName);

        //string json = JsonConvert.SerializeObject(player);

        if (!File.Exists(SavePath))
        {
            Debug.Log("Create new file: " + SavePath);
        }

        //File.WriteAllText(SavePath, json);
        //string json = Json.SerializeObject(player, false);
        Json.SaveJson(asset, SavePath, false);
        //string str = File.ReadAllText(SavePath);
        

        //PlayerInfo result = JsonConvert.DeserializeObject<PlayerInfo>(str);
   
        Json.LoadJson(SavePath, out AssetCustom result, false);
        if (result != null)
        {
            Debug.Log($"AssetCustom.Name: {result.Name} and AssetCustom.GUILD:{result.PATH}");
        }
        else
        {
            Debug.Log("Can't serializa object!");
        }

        Json.LoadJson(pathFile, out DataSave save, false);
        if (result != null)
        {
            Debug.Log($"AssetCustom.Name: {save.FPS} and AssetCustom.GUILD:{save.CurrentLocalized}");
        }
        else
        {
            Debug.Log("Can't serializa object!");
        }

        var obj = Addressables.InstantiateAsync(result.PATH);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
