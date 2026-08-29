using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class CharacterAvatar : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image border;
    [SerializeField] private GameObject lockedIcon;

    private string id;
    private string weaponID;
    public string WeaponID { get { return weaponID; }  set { weaponID = value; } } 
    private AudioManager audioManager;
    public string ID => id;

    private bool isShowWeaponCategory = false;
    public bool IsShowWeaponCategory { get { return isShowWeaponCategory; } set { isShowWeaponCategory = value; } }

    private bool isUnlocked = true;
    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        AutoWire();
    }

    private void AutoWire()
    {
        if (lockedIcon == null)
        {
            var t = transform.Find("loced_icon");
            if (t == null) t = transform.Find("locked_icon");
            if (t == null) t = transform.Find("Locked_Icon");
            if (t != null) lockedIcon = t.gameObject;
        }
    }

    public void Init(string id, string weapon, Sprite icon, AudioManager audio, bool isUnlocked = true)
    {
        this.id = id;
        if (this.icon != null) this.icon.sprite = icon;
        this.weaponID = weapon;
        this.audioManager = audio;
        this.isUnlocked = isUnlocked;

        AutoWire();

        if (lockedIcon != null)
        {
            lockedIcon.SetActive(!isUnlocked);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectCharacterAvatar?.Invoke(id);
        if (isShowWeaponCategory)
        {
            HandleOnClickEvent();
        }
        if (audioManager != null)
        {
            audioManager.PlaySFXAsync(id, true).Forget();
        }
    }

    public void SwitchStatus(bool value)
    {
        if (border != null)
        {
            border.color = value ? Definition.SeletedColor : Definition.OriginColor;
        }
        this.transform.localScale = value ? Definition.scale : Vector3.one;
    }

    public void HandleOnClickEvent()
    {
        UIEvent.OnSelectCharacterChangeWeapon?.Invoke(weaponID);
    }
}
