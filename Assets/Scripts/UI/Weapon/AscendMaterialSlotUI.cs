using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AscendMaterialSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private GameObject buttonIcon;
    [SerializeField] private GameObject bg;

    public string SelectedWeaponUUID { get; private set; }
    public Action OnSlotClicked;

    private void Start()
    {
        SetWeaponEmpty();
    }

    public void SetWeaponEmpty()
    {
        SelectedWeaponUUID = "";
        SwitchStatus(true);
    }

    public void SetWeaponImage(string uuid, Sprite sprite)
    {
        SelectedWeaponUUID = uuid;
        if (weaponImage != null) weaponImage.sprite = sprite;
        SwitchStatus(false);
    }

    private void SwitchStatus(bool isEmpty)
    {
        if (buttonIcon != null) buttonIcon.SetActive(isEmpty);
        if (bg != null) bg.SetActive(isEmpty);
        if (weaponImage != null) weaponImage.gameObject.SetActive(!isEmpty);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke();
    }
}
