using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject waeponUpgradeCard;
    [SerializeField] private GameObject weaponAscendCard;

    private void OnEnable()
    {
        UIEvent.OnSelectToggleWeaponTap += ShowWeaponByToggleTap;
        //ShowWeaponByToggleTap(WeaponTap.Upgarde);
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleWeaponTap -= ShowWeaponByToggleTap;
    }
    public void ShowWeaponByToggleTap(WeaponTap type)
    {
        switch(type)
        {
            case WeaponTap.Upgarde:
                {
                    waeponUpgradeCard.gameObject.SetActive(true);
                    weaponAscendCard.gameObject.SetActive(false);
                }
                break;
            case WeaponTap.Ascend:
                {
                    waeponUpgradeCard.gameObject.SetActive(false);
                    weaponAscendCard.gameObject.SetActive(true);
                }
                break;
        }
    }
}
