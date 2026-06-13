using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image imgBackground;
    [SerializeField] private Image imgIcon;

    [Header("Background Config by Rarity")]
    [Tooltip("Index 0 = Normal, 1 = Rare, 2 = Epic, 3 = Legendary (SSR)")]
    [SerializeField] private Sprite[] rarityBackgrounds = new Sprite[4];

    public RectTransform RectTransform => (RectTransform)transform;

    public void Setup(string itemName, Sprite icon, Rare rarity)
    {
        if (imgIcon != null)
            imgIcon.sprite = icon;

        if (imgBackground != null)
        {
            // Map Rare enum to index (0 to 3)
            int index = GetRarityIndex(rarity);
            if (index >= 0 && index < rarityBackgrounds.Length && rarityBackgrounds[index] != null)
            {
                imgBackground.sprite = rarityBackgrounds[index];
            }
        }
    }

    private int GetRarityIndex(Rare rarity)
    {
        int val = (int)rarity;
        // Map 2-5 -> 0-3 (Common -> Uncommon -> Rare -> Legend)
        if (val >= 2 && val <= 5)
        {
            return val - 2;
        }
        return 0;
    }
}
