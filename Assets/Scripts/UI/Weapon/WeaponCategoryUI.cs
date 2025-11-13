using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class WeaponCategoryUI : GameItemUI,IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private GameObject parent;
    private UpgradeUI[] upgrades;
    [SerializeField] private Image avatarIcon;

    private void Awake()
    {
        upgrades = parent.GetComponentsInChildren<UpgradeUI>();
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, Sprite avatar, int level, int upgradeNumber)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = level.ToString();
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (i < upgradeNumber)
            {
                upgrades[i].ActiveLayer(1);
            }
            else
            {
                upgrades[i].ActiveLayer(0);
            }
        }

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
