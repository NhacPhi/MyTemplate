using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : InventoryItemUI
{
    [SerializeField] private TextMeshProUGUI txtNumber;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int number)
    {
        base.Setup(id, rare, icon, background);
        txtNumber.text = number.ToString();
    }

}
