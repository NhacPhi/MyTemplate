using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ArmorCardInforUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtNameItem;


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
        UIEvent.OnSelectInventoryItem += UpdateArmorItemCardInfor;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateArmorItemCardInfor;
    }

    public void UpdateArmorItemCardInfor(string id)
    {
        Armor item = save.Player.GetArmor(id);
        BaseArmorConfig config = itemData.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, item.TemplateID);
        ArmorSO armorSO = itemData.GetItemSOByID<ArmorSO>(ItemType.Armor, item.TemplateID);

        txtNameItem.text = Utility.GetArmorPartName(config.Part) + " " + Utility.GetArmorRaretName(item.Rare) + "-" + LocalizationManager.Instance.GetLocalizedValue(config.Name);

        armor.Init(item.InstanceID, item.Rare, armorSO.Icon, itemData.GetRareBG(item.Rare), item.Level);
    }
}
