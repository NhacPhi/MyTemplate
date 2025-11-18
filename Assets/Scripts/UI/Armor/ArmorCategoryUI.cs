using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArmorCategoryUI : GameItemUI, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI txtLevel;
    [SerializeField] Image avatarIcon;

    private ArmorPart part;
    public ArmorPart Part => part;
    public void Init(string id, Rare rare, Sprite icon, Sprite background, Sprite avatar, int level,ArmorPart part)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = "+" + level.ToString();
        if (avatar == null)
        {
            avatarIcon.gameObject.SetActive(false);
        }
        else
        {
            avatarIcon.gameObject.SetActive(true);
            avatarIcon.sprite = avatar;
        }
        this.part = part;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnClickArmorCategoryUI?.Invoke(id);
        UIEvent.OnShowTooltipUI?.Invoke(true);
        UIEvent.OnUpdateArmorTooltipUI?.Invoke(id);
    }
}
