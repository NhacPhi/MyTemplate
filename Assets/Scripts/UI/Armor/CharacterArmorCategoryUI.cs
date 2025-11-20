using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VContainer;
using static UnityEditor.Progress;
using UnityEditor.Experimental.GraphView;

public class CharacterArmorCategoryUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    [SerializeField] private GameObject prefabsUI;
    [SerializeField] private GameObject content;

    [SerializeField] private List<ArmorPartToggle> toggles;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;

    private List<GameObject> armors = new();

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
    }


    private void OnDisable()
    {
        UIEvent.OnClickArmorIconCatergory += UpdateCategoryArmor;
    }
    private void OnDestroy()
    {
        UIEvent.OnUpdateCharacterCategoryArmor -= UpdateCategoryArmorAndToggles;
    }

    private void Init()
    {
        foreach(var armor in save.Player.Armors)
        {
            var obj = Instantiate(prefabsUI, content.transform);
            var arrmorConfig = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, armor.TemplateID);
            var armorSO = gameDataBase.GetItemSOByID<ArmorSO>(ItemType.Armor, armor.TemplateID);
            Sprite avatar = armor.Equip != "None" ? gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard, "shard_" + armor.Equip).Icon : null;
            obj.GetComponent<ArmorCategoryUI>().Init(armor.InstanceID, armor.Rare, armorSO.Icon, gameDataBase.GetRareBG(armor.Rare), avatar, armor.Level, arrmorConfig.Part);
            obj.SetActive(false);
            armors.Add(obj);
        }
    }
    
    private void UpdateCategoryArmorAndToggles(ArmorPart part)
    {
        UpdateCategoryArmor(part);
        UpdateArmorPartToggles(part);
    }

    private void UpdateCategoryArmor(ArmorPart part)
    {
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
        foreach(var armor in armors)
        {
            ArmorCategoryUI armorUI = armor.GetComponent<ArmorCategoryUI>();
            if(armorUI.Part == part)
            {
                armor.gameObject.SetActive(true);
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
}
