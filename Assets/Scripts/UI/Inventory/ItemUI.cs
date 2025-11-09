using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : InventoryItemUI
{
    [SerializeField] private TextMeshProUGUI txtNumber;

    [SerializeField] private GameObject fragIcon;

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int number)
    {
        base.Setup(id, rare, icon, background);
        txtNumber.text = number.ToString();
    }

    public void ActiveFragIcon(bool shard)
    {
        fragIcon.SetActive(shard);
    }

}
