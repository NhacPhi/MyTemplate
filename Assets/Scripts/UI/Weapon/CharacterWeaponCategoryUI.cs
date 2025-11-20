using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VContainer;

public class CharacterWeaponCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    [SerializeField] private GameObject prefabsUI;
    [SerializeField] private GameObject content;


    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;

    private List<GameObject> weapons = new();

    private void Awake()
    {
        UIEvent.OnSelectCharacterChangeWeapon += ResetWeaponCardCategory;
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += SelectedWeaponCard;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= SelectedWeaponCard;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterChangeWeapon -= ResetWeaponCardCategory;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(() =>
        {
            UIEvent.OnCloseCharacterWeapon?.Invoke(true);
            UIEvent.OnSelectToggleCharacterTap?.Invoke(CharacterTap.Relic);
        });
        Init();

        ResetWeaponCardCategory(save.Player.GetIDOfFirstCharacter().Weapon);
    }

    public void Init()
    {
        foreach (var item in save.Player.Weapons)
        {
            var obj = Instantiate(prefabsUI, content.transform);
            var weaponConfig = gameDataBase.GetItemConfigByID<WeaponConfig>(ItemType.Weapon, item.ID);
            var weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, item.ID);
            Sprite avatar = item.Equip != "None" ? gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard, "shard_" + item.Equip).Icon : null;
            obj.GetComponent<WeaponCategoryUI>().Init(item.ID, weaponConfig.Rare, weaponSO.Icon, gameDataBase.GetRareBG(weaponConfig.Rare), avatar, item.CurrentLevel, item.CurrentUpgrade);
            obj.SetActive(true);
            weapons.Add(obj);
        }

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void SelectedWeaponCard(string id)
    {
        ResetWeaponCards();
    }

    private void ResetWeaponCards()
    {
        foreach (var weapon in weapons)
        {
            weapon.GetComponent<WeaponCategoryUI>().OnSwitchStatusBoder(false);
        }
    }

    public void ResetWeaponCardCategory(string id)
    {
        if (id == "")
        {
            foreach (var weapon in weapons)
            {
                weapon.GetComponent<WeaponCategoryUI>().OnSwitchStatusBoder(false);
            }
        }
        else
        {
            foreach (var weapon in weapons)
            {
                weapon.GetComponent<WeaponCategoryUI>().OnSwitchStatusBoder(false);

                if (weapon.GetComponent<WeaponCategoryUI>().ID == id)
                {
                    weapon.GetComponent<WeaponCategoryUI>().OnSwitchStatusBoder(true);
                }
            }
        }
    }
} 
