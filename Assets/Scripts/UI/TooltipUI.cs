using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject) continue;

            var armorUI = result.gameObject.GetComponentInParent<ArmorCategoryUI>();
            if (armorUI != null)
            {
                armorUI.OnPointerClick(eventData);
                return;
            }

            var weaponUI = result.gameObject.GetComponentInParent<WeaponCategoryUI>();
            if (weaponUI != null)
            {
                weaponUI.OnPointerClick(eventData);
                return;
            }
        }

        gameObject.SetActive(false);
        UIEvent.OnHideAllToolTipUI?.Invoke();
    }
}
