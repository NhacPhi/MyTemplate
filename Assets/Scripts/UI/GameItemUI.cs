using UnityEngine.UI;
using UnityEngine;

public class GameItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private Image boder;
    private Rare rare;
    private Color color;

    protected string id;
    public string ID => id;

    private bool canClick = true;
    public bool CanClick
    {
        get { return canClick; }
        set { canClick = value; }
    }

    public virtual void Setup(string id, Rare rare, Sprite icon, Sprite background)
    {
        this.id = id;
        this.icon.sprite = icon;
        this.background.sprite = background;
        this.rare = rare;
        SetBoderFollowRare();
        color = boder.color;
    }

    public void OnSwitchStatusBoder(bool selected)
    {
        if (canClick)
        {
            boder.color = selected ? Definition.SeletedColor : color;
        }
    }

    private void SetBoderFollowRare()
    {
        switch (rare)
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
