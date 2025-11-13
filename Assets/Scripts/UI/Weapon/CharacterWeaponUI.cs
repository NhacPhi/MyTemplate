using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterWeaponUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image weapon;
    [SerializeField] private GameObject buttonIcon;
    [SerializeField] private GameObject bg;

    private bool canClick;
    public bool CanClick { get { return canClick; } set { canClick = value; }}
    // Start is called before the first frame update
    void Start()
    {
        canClick = true;
    }

    public void SetWeaponEmpty()
    {
        SwitchStatus(true);
    }

    public void SetWeaponImage(Sprite prite)
    {
        weapon.sprite = prite;
        SwitchStatus(false);
    }

    private void SwitchStatus(bool status)
    {
        canClick = status;
        buttonIcon.SetActive(status);
        bg.SetActive(status);
        weapon.gameObject.SetActive(!status);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(canClick)
        {
            //Open weapon menu
            UIEvent.OnSelectToggleCharacterTap?.Invoke(CharacterTap.Relic);
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke("");
        }
    }
}
