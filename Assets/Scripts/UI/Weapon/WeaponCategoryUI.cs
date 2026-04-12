using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class WeaponCategoryUI : GameItemUI,IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgrades;
    [SerializeField] private Image avatarIcon;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, Sprite avatar, int level, int upgradeNumber)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = level.ToString();
        upgrades.UpdateUI(upgradeNumber);
        

        if(avatar == null)
        {
            avatarIcon.gameObject.SetActive(false);
        }
        else
        {
            avatarIcon.gameObject.SetActive(true);
            avatarIcon.sprite = avatar;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //UIEvent.OnSelectInventoryItem?.Invoke(id);
        UIEvent.OnSelectWeaponCard?.Invoke(id);
        OnSwitchStatusBoder(true);
    }

    //public void SelectedWeaponCardUI()
    //{
    //    UIEvent.OnSelectWeaponCard?.Invoke(id);
    //    OnSwitchStatusBoder(true);
    //}
}
