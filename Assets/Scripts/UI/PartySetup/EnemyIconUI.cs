using UnityEngine;
using TMPro;

public class EnemyIconUI : GameItemUI
{
    [SerializeField] private TextMeshProUGUI txtLevel;

    public void Setup(string id, CharacterRare characterRare, Sprite icon, Sprite background, int level)
    {
        Rare rare = ConvertToItemRare(characterRare);
        base.Setup(id, rare, icon, background);
        SetLevel(level);
    }

    public void Setup(string id, Rare rare, Sprite icon, Sprite background, int level)
    {
        base.Setup(id, rare, icon, background);
        SetLevel(level);
    }

    public void Init(string id, CharacterRare characterRare, Sprite icon, Sprite background, int level)
    {
        Setup(id, characterRare, icon, background, level);
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level)
    {
        Setup(id, rare, icon, background, level);
    }

    public void SetLevel(int level)
    {
        if (txtLevel != null)
        {
            txtLevel.text = level > 0 ? level.ToString() : string.Empty;
        }
    }

    public static Rare ConvertToItemRare(CharacterRare characterRare)
    {
        switch (characterRare)
        {
            case CharacterRare.UR:
            case CharacterRare.SSR:
                return Rare.Legendary;
            case CharacterRare.SR:
                return Rare.Rare;
            case CharacterRare.R:
            default:
                return Rare.Common;
        }
    }
}
