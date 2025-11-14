using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterArmorUI : GameItemUI,IPointerClickHandler
{
    [SerializeField] private ArmorPart type;

    [SerializeField] TextMeshProUGUI txtLevel;
    [SerializeField] Image emptyIcon;
    private bool isEmpty;
    public bool IsEmpty { get { return isEmpty; } set { isEmpty = value; } }
    public ArmorPart Type => type;

    private void Start()
    {
        IsEmpty = true;
    }
    public void UpdateArmorUI(string id, Rare rare, Sprite icon, Sprite background, int level)
    {
        emptyIcon.gameObject.SetActive(false);
        base.Setup(id, rare, icon, background);
        txtLevel.text = "+" + level.ToString();
    }

    public void SwitchStatusArmorUI(bool empty)
    {
       emptyIcon.gameObject.SetActive(empty);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnShowCharacterCategoryArmor?.Invoke(type);
        UIEvent.OnUpdateCharacterCategoryArmor?.Invoke(type);
        UIEvent.OnSelectCharacterArmorUI?.Invoke(ID);
        OnSwitchStatusBoder(true);
    }
}
