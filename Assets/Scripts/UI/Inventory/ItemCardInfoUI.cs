using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ItemCardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtOwned;
    [SerializeField] private TextMeshProUGUI txtUseful;
    [SerializeField] private TextMeshProUGUI txtDes;

    [SerializeField] private GameObject content;

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private GameDataBase gameDataBase;
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
        ItemData item = save.Player.GetItem(id);
        ItemBaseConfig itemConfig = default;
        ItemBaseSO itemSO = default;
        string str = "";
        switch (item.Type)
        {
            case ItemType.Food:
                {
                    FoodConfig config = gameDataBase.GetItemConfigByID<FoodConfig>(ItemType.Food, id);
                    itemSO = gameDataBase.GetItemSOByID<FoodSO>(ItemType.Food, id);
                    itemConfig = config;
                }
            break;
            case ItemType.GemStone:
                {
                    GemStoneConfig config = gameDataBase.GetItemConfigByID<GemStoneConfig>(ItemType.GemStone, id);
                    itemSO = gameDataBase.GetItemSOByID<GemStoneSO>(ItemType.GemStone, id);
                    itemConfig = config;
                }
                break;
            case ItemType.Shard:
                {
                    ShardConfig config = gameDataBase.GetItemConfigByID<ShardConfig>(ItemType.Shard, id);
                    itemSO = gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard, id);
                    itemConfig = config;
                }
                break;
            case ItemType.Exp:
                {
                    ExpConfig config = gameDataBase.GetItemConfigByID<ExpConfig>(ItemType.Exp, id);
                    itemSO = gameDataBase.GetItemSOByID<ItemBaseSO>(ItemType.Exp, id);
                    str = string.Format(LocalizationManager.Instance.GetLocalizedValue(config.Useful), config.Exp);
                    itemConfig = config;
                }
                break;
        }
        txtUseful.text = item.Type == ItemType.Exp ? str : LocalizationManager.Instance.GetLocalizedValue(itemConfig.Useful);
        icon.sprite = itemSO.Icon;
        txtName.text = (item.Type == ItemType.Shard ? (LocalizationManager.Instance.GetLocalizedValue("STR_SHARD_NAME") + " " ) : "") + LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
        txtOwned.text = item.Quanlity.ToString();
        txtDes.text = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Description);


        LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(txtUseful.rectTransform);

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }
}
