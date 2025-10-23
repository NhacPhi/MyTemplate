using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class ItemCardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtOwned;
    [SerializeField] private TextMeshProUGUI txtUseful;
    [SerializeField] private TextMeshProUGUI txtDes;

    [SerializeField] private GameObject content;

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private ItemDataBase itemData;
    [Inject] private SaveSystem save;
    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }
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
        Item item = save.Player.GetItem(id);
        ItemBaseConfig itemConfig = default;
        ItemBaseSO itemSO = default;
        switch(item.Type)
        {
            case ItemType.Food:
                {
                    FoodConfig config = itemData.GetItemConfigByID<FoodConfig>(ItemType.Food, id);
                    itemSO = itemData.GetItemSOByID<FoodSO>(ItemType.Food, id);
                    txtUseful.text = LocalizationManager.Instance.GetLocalizedValue(config.Useful);
                    itemConfig = config;
                }
            break;
        }

        icon.sprite = itemSO.Icon;
        txtName.text = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
        txtOwned.text = item.Count.ToString();
        txtDes.text = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Description);


        LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(txtUseful.rectTransform);

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }
}
