using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private Image boder;
    private Rare rare;
    private Color color;

    private string id;
    public string ID => id;

    public virtual void Setup(string id, Rare rare , Sprite icon, Sprite background)
    {
        this.id = id;
        this.icon.sprite = icon;
        this.background.sprite = background;
        this.rare = rare;
        SetBoderFollowRare();
        color = boder.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectInventoryItem?.Invoke(id);
        OnSwitchStatusBoder(true);
    }

    public void OnSwitchStatusBoder(bool selected)
    {
        boder.color = selected ? Definition.SeletedColor : color;
    }

    private void SetBoderFollowRare()
    {
        switch(rare)
        {
            case Rare.Common:
                boder.color = Definition.CommonColor;
                break;
            case Rare.Uncommon:
                boder.color = Definition.UncommonColor;
                break;
            case Rare.Rare:
                boder.color = Definition.RareColor;
                break;
            case Rare.Epic:
                boder.color = Definition.EpicColor;
                break;
            case Rare.Legendary:
                boder.color = Definition.LegendaryColor;
                break;
        }
    }
}
