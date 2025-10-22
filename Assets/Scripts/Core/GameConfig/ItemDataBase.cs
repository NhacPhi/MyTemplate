using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
public class ItemDataBase : MonoBehaviour
{
    [SerializeField] private List<AvatarIconSO> avatars;
    [SerializeField] private List<WeaponSO> weapons;
    [SerializeField] private List<RareSO> rares;

    private List<AvatarConfig> avatarConfigs;
    private List<WeaponConfig> weaponConfig;

    public List<AvatarIconSO> Avatars { get { return avatars; } }
    public List<WeaponSO> Weapons => weapons;
    public List<RareSO> Rares => rares;
    public List<AvatarConfig> AvatarConfigs { get { return avatarConfigs; } }
    public List<WeaponConfig> WeaponConfigs => weaponConfig;


    private void Start()
    {
        LoadItemDataConfig();
    }

    private void LoadItemDataConfig()
    {
        string path = "Assets/Data/GameConfig/";

        string fileAvatar = "AvatarConfig.json";

        string fileWeapon = "Weapon.json";

        Json.LoadJson(Path.Combine(path,fileAvatar), out avatarConfigs);
        Json.LoadJson(Path.Combine(path, fileWeapon), out weaponConfig);
    }

    public WeaponConfig GetWeaponConfig(string id)
    {
        return weaponConfig.Find(weapon => weapon.ID == id);
    }

    public WeaponSO GetWeaponSO(string id)
    {
        return weapons.Find(weapon => weapon.ID == id);
    }

    public Sprite GetRareBG(Rare type)
    {
        return rares.Find(v => v.Type == type).Image;
    }
}
