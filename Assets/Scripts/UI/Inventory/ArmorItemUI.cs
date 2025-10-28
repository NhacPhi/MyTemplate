using TMPro;
using UnityEngine;

public class ArmorItemUI : InventoryItemUI
{
    [SerializeField] TextMeshProUGUI txtLevel;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = "+" + level.ToString();
    }
}
