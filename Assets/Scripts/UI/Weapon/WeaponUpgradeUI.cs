using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class WeaponUpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject waeponUpgradeCard;
    [SerializeField] private GameObject weaponAscendCard;
    [SerializeField] private AscendMaterialCategoryUI categoryUI;
    [SerializeField] private Image weaponIconBig;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventory;

    private void OnEnable()
    {
        UIEvent.OnSelectToggleWeaponTap += ShowWeaponByToggleTap;
        UIEvent.OnSlelectWeaponEnchance += UpdateWeaponIcon;
        //ShowWeaponByToggleTap(WeaponTap.Upgarde);
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleWeaponTap -= ShowWeaponByToggleTap;
        UIEvent.OnSlelectWeaponEnchance -= UpdateWeaponIcon;
    }

    private void UpdateWeaponIcon(string weaponUUID)
    {
        if (weaponIconBig != null && !string.IsNullOrEmpty(weaponUUID))
        {
            var weaponSave = inventory.GetWeapon(weaponUUID);
            if (weaponSave != null)
            {
                var config = gameDataBase.GetItemConfig(weaponSave.TemplateID);
                if (config != null && config.Weapon != null)
                {
                    weaponIconBig.sprite = config.Weapon.BigIcon;
                }
            }
        }
    }
    public void ShowWeaponByToggleTap(WeaponTap type)
    {
        switch(type)
        {
            case WeaponTap.Upgarde:
                {
                    waeponUpgradeCard.gameObject.SetActive(true);
                    weaponAscendCard.gameObject.SetActive(false);
                    categoryUI.gameObject.SetActive(false);
                }
                break;
            case WeaponTap.Ascend:
                {
                    waeponUpgradeCard.gameObject.SetActive(false);
                    weaponAscendCard.gameObject.SetActive(true);
                    categoryUI.gameObject.SetActive(true);
                }
                break;
        }
    }
}
