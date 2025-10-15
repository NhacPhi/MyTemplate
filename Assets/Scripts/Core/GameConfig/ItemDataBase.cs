using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
public class ItemDataBase : MonoBehaviour
{
    [SerializeField] private List<AvatarIconSO> avatars;

    private List<Avatar> avatarConfigs;

    public List<AvatarIconSO> Avatars { get { return avatars; } }
    public List<Avatar> AvatarConfigs { get { return avatarConfigs; } }


    private void Start()
    {
        LoadItemDataConfig();
    }

    private void LoadItemDataConfig()
    {
        string path = "Assets/Data/GameConfig/";

        string fileAvatar = "Avatar.json";

       Json.LoadJson(Path.Combine(path,fileAvatar), out avatarConfigs);
    }
}
