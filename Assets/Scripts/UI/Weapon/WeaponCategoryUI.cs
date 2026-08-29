using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponCategoryUI : GameItemUI, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgrades;
    [SerializeField] private Image avatarIcon;
    [SerializeField] private GameObject locked;

    public bool IsUnlocked { get; private set; } = true;

    private void Awake()
    {
        EnsureLockedObject();
    }

    private void EnsureLockedObject()
    {
        if (locked == null)
        {
            var t = transform.Find("locked");
            if (t != null) locked = t.gameObject;
        }
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, Sprite avatar, int level, int upgradeNumber, bool unlocked = true)
    {
        base.Setup(id, rare, icon, background);
        IsUnlocked = unlocked;

        EnsureLockedObject();

        if (txtLevel != null)
        {
            txtLevel.gameObject.SetActive(unlocked);
            if (unlocked) txtLevel.text = level.ToString();
        }

        if (upgrades != null)
        {
            upgrades.gameObject.SetActive(unlocked);
            if (unlocked) upgrades.UpdateUI(upgradeNumber);
        }

        if (avatar == null || !unlocked)
        {
            if (avatarIcon != null) avatarIcon.gameObject.SetActive(false);
        }
        else
        {
            if (avatarIcon != null)
            {
                avatarIcon.gameObject.SetActive(true);
                avatarIcon.sprite = avatar;
            }
        }

        if (locked != null)
        {
            locked.SetActive(!unlocked);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectWeaponCard?.Invoke(id);
        OnSwitchStatusBoder(true);
    }
}
