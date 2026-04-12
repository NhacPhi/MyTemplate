using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UnityEngine.UI;
using System;

public class AscendMaterialCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;
    [SerializeField] private WeaponCategoryUI prefabsUI;
    [SerializeField] private GameObject content;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private ForgeManager forgeManager;
    [Inject] private InventoryManager inventory;

    private List<WeaponCategoryUI> weapons = new();
    
    public Action<string> OnMaterialSelected;

    private void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(CloseCategory);
    }

    public void Init(string targetWeaponUUID)
    {
        RefreshUI(targetWeaponUUID);
    }

    public void CloseCategory()
    {
        gameObject.SetActive(false);
    }

    private void RefreshUI(string targetWeaponUUID)
    {
        if (forgeManager == null) return;

        var duplicates = forgeManager.GetDuplicateWeapons(targetWeaponUUID);
        
        for (int i = 0; i < duplicates.Count; i++)
        {
            var item = duplicates[i];

            WeaponCategoryUI weaponUI;

            if (i < weapons.Count)
            {
                weaponUI = weapons[i];
                weaponUI.gameObject.SetActive(true);
            }
            else
            {
                var obj = Instantiate(prefabsUI, content.transform);
                weaponUI = obj.GetComponent<WeaponCategoryUI>();
                weapons.Add(weaponUI);
            }

            var weaponConfig = gameDataBase.GetItemConfig(item.TemplateID);
            Sprite avatar = item.Equip != "" ? gameDataBase.GetCharacterConfig(item.Equip).Icon : null;
            weaponUI.Init(item.UUID, weaponConfig.Rarity, weaponConfig.Icon,
                weaponConfig.IconBG, avatar, item.CurrentLevel, item.CurrentUpgrade);
            
            weaponUI.OnSwitchStatusBoder(false);
        }

        for (int i = duplicates.Count; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += HandleSelectWeaponCard;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= HandleSelectWeaponCard;
    }

    private void HandleSelectWeaponCard(string uuid)
    {
        var exists = weapons.Find(w => w.gameObject.activeSelf && w.ID == uuid);
        if (exists != null)
        {
            OnMaterialSelected?.Invoke(uuid);
        }
    }

    public void ToggleMaterialVisibility(string uuid, bool isVisible)
    {
        var weapon = weapons.Find(w => w.ID == uuid);
        if (weapon != null)
        {
            weapon.gameObject.SetActive(isVisible);
        }
    }
}
