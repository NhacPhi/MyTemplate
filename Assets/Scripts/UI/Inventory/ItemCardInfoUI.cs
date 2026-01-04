using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static UnityEditor.Progress;

public class ItemCardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtOwned;
    [SerializeField] private TextMeshProUGUI txtUseful;
    [SerializeField] private TextMeshProUGUI txtDes;

    [SerializeField] private GameObject content;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;

    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateItemCardInfor;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateItemCardInfor;
    }
    public void UpdateItemCardInfor(string id)
    {
        ItemSaveData item = save.Player.GetItem(id);
        var itemConfig = gameDataBase.GetItemConfig(id);
        string str = "";
        if(itemConfig != null)
        {

            if(itemConfig.Type == ItemType.Exp)
            {
                str = string.Format(LocalizationManager.Instance.GetLocalizedValue(itemConfig.UseDescription), itemConfig.Exp.Value);
            }

            txtUseful.text = item.Type == ItemType.Exp ? str : LocalizationManager.Instance.GetLocalizedValue(itemConfig.UseDescription);
            icon.sprite = itemConfig.Icon;
            txtName.text = (item.Type == ItemType.Shard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " ") : "") + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
            txtOwned.text = item.Quantity.ToString();
            txtDes.text = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Description);


            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtUseful.rectTransform);

            // Force rebuild UI layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }
    }
}
