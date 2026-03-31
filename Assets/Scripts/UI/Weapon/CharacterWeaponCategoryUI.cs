using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VContainer;
using System.Linq;
using static UnityEditor.Progress;

public class CharacterWeaponCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    [SerializeField] private WeaponCategoryUI prefabsUI;
    [SerializeField] private GameObject content;


    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventory;

    private List<WeaponCategoryUI> weapons = new();

    private void Awake()
    {
        UIEvent.OnSelectCharacterChangeWeapon += ResetWeaponCardCategory;
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += SelectedWeaponCard;
        UIEvent.OnUpdateSingleWeaponCard += UpdateSingleUI;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= SelectedWeaponCard;
        UIEvent.OnUpdateSingleWeaponCard -= UpdateSingleUI;
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

        ResetWeaponCardCategory(inventory.Weapons.FirstOrDefault().UUID);
    }

    public void Init()
    {
        foreach (var item in inventory.Weapons)
        {
            var obj = Instantiate(prefabsUI, content.transform);
            var weaponConfig = gameDataBase.GetItemConfig(item.TemplateID);

            Sprite avatar = item.Equip != "" ? gameDataBase.GetCharacterConfig(item.Equip).Icon : null;
            obj.GetComponent<WeaponCategoryUI>().Init(item.UUID, weaponConfig.Rarity, weaponConfig.Icon, weaponConfig.IconBG, avatar, item.CurrentLevel, item.CurrentUpgrade);
            obj.gameObject.SetActive(true);
            weapons.Add(obj);
        }

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    // Thiết kế với mấy object pooling
    public void RefreshUI()
    {
        var inventoryWeapons = inventory.Weapons;

        for(int i = 0; i < inventoryWeapons.Count; i++)
        {
            var item = inventoryWeapons[i];

            WeaponCategoryUI weaponUI;

            if(i < weapons.Count)
            {
                weaponUI = weapons[i];
                weaponUI.gameObject.SetActive(true);
            }
            else
            {
                var obj = Instantiate(prefabsUI, content.transform);
                weaponUI = obj.GetComponent<WeaponCategoryUI>();
                weaponUI.gameObject.SetActive(true);
            }

            //Init data
            var weaponConfig = gameDataBase.GetItemConfig(item.TemplateID);
            Sprite avatar = item.Equip != "" ? gameDataBase.GetCharacterConfig(item.Equip).Icon : null;
            weaponUI.Init(item.UUID, weaponConfig.Rarity, weaponConfig.Icon,
                weaponConfig.IconBG, avatar, item.CurrentLevel, item.CurrentUpgrade);
        }

        for(int i = inventoryWeapons.Count; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }
    // Nếu số lượng object quá lớn thì việc tiêm data cho tất cả khá tốn CPU
    // Update data cho đúng UI đó
    public void UpdateSingleUI(string uuid)
    {
        var weaponUI = weapons.Find(x => x.ID == uuid);
        if(weaponUI != null) 
        {
            var itemData = inventory.GetWeapon(uuid);
            var weaponConfig = gameDataBase.GetItemConfig(itemData.TemplateID);
            Sprite avatar = itemData.Equip != "" ? gameDataBase.GetCharacterConfig(itemData.Equip).Icon : null;
            weaponUI.Init(itemData.UUID, weaponConfig.Rarity, weaponConfig.Icon,
                weaponConfig.IconBG, avatar, itemData.CurrentLevel, itemData.CurrentUpgrade);
        }
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
