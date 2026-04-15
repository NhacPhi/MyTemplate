using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VContainer;
using static UnityEditor.Progress;


public class CharacterArmorCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    [SerializeField] private ArmorCategoryUI prefabsUI;
    [SerializeField] private GameObject content;

    [SerializeField] private List<ArmorPartToggle> toggles;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventory;

    private List<ArmorCategoryUI> armors = new();

    private ArmorPart currentPart = ArmorPart.Helmet;

    private void Awake()
    {
        UIEvent.OnUpdateCharacterCategoryArmor += UpdateCategoryArmorAndToggles;
        Init();
    }
    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(() =>
        {
            UIEvent.OnCloseCharacterCategoryArmor?.Invoke();
        });
    }
    private void OnEnable()
    {
        UIEvent.OnClickArmorIconCatergory += UpdateCategoryArmor;
        UIEvent.OnUpdateSingleArmorPart += UpdateSingleUI;
        UIEvent.OnSelectCharacterAvatar += UpdateCurrentArmorPart;
        UIEvent.OnCloseUpgradeArmorScene += OnArmorSceneClosed;
        RefreshUI();
    }


    private void OnDisable()
    {
        UIEvent.OnClickArmorIconCatergory -= UpdateCategoryArmor;
        UIEvent.OnUpdateSingleArmorPart -= UpdateSingleUI;
        UIEvent.OnSelectCharacterAvatar -= UpdateCurrentArmorPart;
        UIEvent.OnCloseUpgradeArmorScene -= OnArmorSceneClosed;
    }
    private void OnDestroy()
    {
        UIEvent.OnUpdateCharacterCategoryArmor -= UpdateCategoryArmorAndToggles;
    }

    private void Init()
    {
        foreach(var armor in inventory.Armors)
        {
            var obj = Instantiate(prefabsUI, content.transform);
            var armorConfig = gameDataBase.GetItemConfig(armor.TemplateID);
            Sprite avatar = armor.Equip != "" ? gameDataBase.GetCharacterConfig(armor.Equip).Icon : null;
            obj.Init(armor.UUID, armor.Rare, armorConfig.Icon, gameDataBase.GetBGItemByRare(armor.Rare), avatar, armor.Level, armorConfig.Armor.Part);
            obj.gameObject.SetActive(false);
            armors.Add(obj);
        }
    }

    private void RefreshUI()
    {
        var inventoryArmors = inventory.Armors;

        for(int i = 0; i < inventoryArmors.Count; i++)
        {
            var item = inventoryArmors[i];

            ArmorCategoryUI armorUI;

            if(i < armors.Count)
            {
                armorUI = armors[i];
                armorUI.gameObject.SetActive(false);
            } 
            else
            {
                var obj = Instantiate(prefabsUI, content.transform);
                armorUI = obj.GetComponent<ArmorCategoryUI>();
                armorUI.gameObject.SetActive(false);
                armors.Add(obj);
            }

            // Init data
            var armorConfig = gameDataBase.GetItemConfig(item.TemplateID);
            Sprite avatar = item.Equip != "" ? gameDataBase.GetCharacterConfig(item.Equip).Icon : null;
            armorUI.Init(item.UUID, item.Rare, armorConfig.Icon, gameDataBase.GetBGItemByRare(item.Rare), avatar, item.Level, armorConfig.Armor.Part);
        }

        for (int i = inventoryArmors.Count; i < armors.Count; i++)
        {
            armors[i].gameObject.SetActive(false);
        }

    }

    public void UpdateSingleUI(string itemUUID)
    {
        var targetUI = armors.Find(ui => ui.ID == itemUUID);

        if(targetUI != null)
        {
            var itemData = inventory.GetArmor(itemUUID);

            if (itemData == null) return;

            var armorConfig = gameDataBase.GetItemConfig(itemData.TemplateID);
            Sprite avatar = itemData.Equip != "" ? gameDataBase.GetCharacterConfig(itemData.Equip).Icon : null;
            targetUI.Init(
                itemData.UUID,
                itemData.Rare,
                armorConfig.Icon,
                gameDataBase.GetBGItemByRare(itemData.Rare),
                avatar,
                itemData.Level,
                armorConfig.Armor.Part
            );
        }
    }
    
    private void UpdateCategoryArmorAndToggles(ArmorPart part)
    {
        UpdateCategoryArmor(part);
        UpdateArmorPartToggles(part);
    }

    private void UpdateCategoryArmor(ArmorPart part)
    {
        currentPart = part;
        DeActiveAllObjectInContent();
        GetArmorByPart(part);
        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private void UpdateArmorPartToggles(ArmorPart part)
    {
        foreach(var toggle in toggles)
        {
            if(toggle.Part == part)
            {
                toggle.ActiveToggle(true);
            }
            else
            {
                toggle.ActiveToggle(false);
            }
        }
    }

    private void GetArmorByPart(ArmorPart part)
    {
        for(int i = 0; i < armors.Count; i++)
        {
            if (i >= inventory.Armors.Count) continue;

            ArmorCategoryUI armorUI = armors[i];

            if (armorUI.Part == part)
            {
                armorUI.gameObject.SetActive(true);
            }

        }    
    }

    private void DeActiveAllObjectInContent()
    {
        ArmorCategoryUI[] objects = content.GetComponentsInChildren<ArmorCategoryUI>();
        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateCurrentArmorPart(string id)
    {
        RefreshUI();
        UpdateCategoryArmor(currentPart);
    }

    private void OnArmorSceneClosed()
    {
        RefreshUI();
        UpdateCategoryArmor(currentPart);
    }
}
