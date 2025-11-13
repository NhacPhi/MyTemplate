using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterAvatar : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image border;

    private string id;
    private string weaponID;
    public string WeaponID { get { return weaponID; }  set { weaponID = value; } } 

    public string ID => id;

    private bool isShowWeaponCategory = false;

    public bool IsShowWeaponCategory { get { return isShowWeaponCategory; } set { isShowWeaponCategory = value; } }

    public void Init(string id,string weapon,Sprite icon)
    {
        this.id = id;
        this.icon.sprite = icon;
        this.weaponID = weapon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectCharacterAvatar?.Invoke(id);
        if (isShowWeaponCategory)
        {
            HandleOnClickEvent();
        }
    }

    public void SwitchStatus(bool value)
    {
        border.color = value ? Definition.SeletedColor : Definition.OriginColor;
        this.transform.localScale = value ? Definition.scale : Vector3.one;
    }

    public void HandleOnClickEvent()
    {
        UIEvent.OnSelectCharacterChangeWeapon?.Invoke(weaponID);
    }
}
