using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GachaResultCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image imgBackground;
    [SerializeField] private Image imgIcon;

    [Header("Background Config by Rarity")]
    [Tooltip("Index 0 = Normal, 1 = Rare, 2 = Epic, 3 = Legendary (SSR)")]
    [SerializeField] private Sprite[] rarityBackgrounds = new Sprite[4];

    [Header("Special Effects (Mức 4 & 5)")]
    [SerializeField] private GameObject fxSparkle;

    [Header("Highlight Shader Materials")]
    [SerializeField] private Material matHighlight;

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

        PlaySpecialEffects(rarity);
    }

    private void PlaySpecialEffects(Rare rarity)
    {
        int val = (int)rarity;

        // Reset trạng thái: Tắt hết các effect đi
        if (fxSparkle != null) fxSparkle.SetActive(false);
        
        // Trả lại material mặc định (không có viền) cho thẻ thường
        if (imgBackground != null)
        {
            imgBackground.material = null;
        }

        // Bật effect lên nếu đúng độ hiếm (Cả 4 và 5 dùng chung)
        if (val == 4 || val == 5)
        {
            if (fxSparkle != null) fxSparkle.SetActive(true);
            
            if (imgBackground != null && matHighlight != null) 
                imgBackground.material = matHighlight; // Bật sáng viền
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
