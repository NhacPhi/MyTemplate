using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class ArmorTooltipUI : MonoBehaviour
{
    [SerializeField] private ArmorItemUI armor;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtScore;

    [SerializeReference] private List<ArmorStatsUI> armorStats;
    [SerializeField] private TextMeshProUGUI txtTitleSet;
    [SerializeField] private TextMeshProUGUI txtDescriptionSet;

    [SerializeField] private Button btnEquip;
    [SerializeField] private Button btnUnequip;
    [SerializeField] private Button btnChange;

    [Inject] GameDataBase gameDataBase;
    [Inject] InventoryManager inventoryManager;
    [Inject] PlayerCharacterManager playerCharacterManager;

    public string CurrentCharacterID = "";
    private string currentArmorPart;

    private void Awake()
    {
        UIEvent.OnUpdateArmorTooltipUI += UpdateArmorUI;
    }


    private void OnEnable()
    {
        UIEvent.OnHideAllToolTipUI += Hide;

        btnChange.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(CurrentCharacterID);
            var armorPartTemplateID = inventoryManager.GetArmor(currentArmorPart).TemplateID;
            var type = gameDataBase.GetItemConfig(armorPartTemplateID).Armor.Part;
            var armorPartOfCharacter = character.SaveData.Armors.Find(p => p.Type == type).ID;
            if (character != null)
            {
                character.ChangeArmor(currentArmorPart);
                UpdateArmorUI(currentArmorPart);
                UIEvent.OnUpdateSingleArmorPart?.Invoke(currentArmorPart);
                UIEvent.OnUpdateSingleArmorPart?.Invoke(armorPartOfCharacter);
                UIEvent.OnSelectCharacterAvatar(CurrentCharacterID);
            }
        });

        btnEquip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(CurrentCharacterID);
            if (character != null)
            {
                character.EquipArmor(currentArmorPart);
                UpdateArmorUI(currentArmorPart);
                UIEvent.OnUpdateSingleArmorPart?.Invoke(currentArmorPart);
                UIEvent.OnSelectCharacterAvatar(CurrentCharacterID);
            }
        });

        btnUnequip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(CurrentCharacterID);
            if (character != null)
            {
                character.UnequipArmor(currentArmorPart);
                UpdateArmorUI(currentArmorPart);
                UIEvent.OnUpdateSingleArmorPart?.Invoke(currentArmorPart);
                UIEvent.OnSelectCharacterAvatar(CurrentCharacterID);
            }
        });
    }

    private void OnDisable()
    {
        UIEvent.OnHideAllToolTipUI -= Hide;
        btnEquip.onClick.RemoveAllListeners();
        btnUnequip.onClick.RemoveAllListeners();
        btnChange.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        UIEvent.OnUpdateArmorTooltipUI -= UpdateArmorUI;
    }


    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateArmorUI(string armorID)
    {
        currentArmorPart = armorID;
        ArmorSaveData armorSaveData = inventoryManager.GetArmor(armorID);
        if (armorSaveData == null) return;

        ItemConfig armorConfig = gameDataBase.GetItemConfig(armorSaveData.TemplateID);
        
        armor.GetComponent<ArmorItemUI>().Init(armorSaveData.UUID, armorSaveData.Rare, armorConfig.Icon, 
            gameDataBase.GetBGItemByRare(armorSaveData.Rare), armorSaveData.Level);
        armor.CanClick = false;

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(armorConfig.Name);
        txtScore.text = "100";

        ResetArmorStatsUI();

        if (armorSaveData.Substats.Count > 0)
        {
            foreach (var obj in armorSaveData.Substats)
            {
                UpdateArmorSubstatsUI(obj);
            }
        }

        var mainstat = gameDataBase.GetItemConfig(armorSaveData.TemplateID).Armor.MainStat;

        var mainstatUI = armorStats.FirstOrDefault(u => u.Type == mainstat.Type);
        mainstatUI.gameObject.SetActive(true);
        mainstatUI.gameObject.transform.SetAsFirstSibling();
        mainstatUI.UpdateStat((int)mainstat.Value, armorSaveData.Level);

        var setBonusConfig = gameDataBase.GetSetBonusConfig(armorConfig.Armor.ArmorSet);

        txtTitleSet.text = string.Format(LocalizationManager.Instance.GetLocalizedValue("STR_SET_BONUS"), setBonusConfig.Pieces);

        txtDescriptionSet.text = string.Format(LocalizationManager.Instance.GetLocalizedValue("UI_SET_BONUS_CONTENT"), 
            Utility.GetContextByStatType(setBonusConfig.Stat), Utility.GetConvertStatValueToString(setBonusConfig.Value, setBonusConfig.Modifier));

        UpdateButtonStates(armorSaveData, armorConfig.Armor.Part, CurrentCharacterID);

        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
    }

    private void UpdateButtonStates(ArmorSaveData armorData, ArmorPart currentPart, string characterID)
    {
        btnEquip.gameObject.SetActive(false);
        btnUnequip.gameObject.SetActive(false);
        btnChange.gameObject.SetActive(false);

        // Kiểm tra xem NHÂN VẬT HIỆN TẠI có đang mặc chính món đồ này không?
        if (armorData.Equip == characterID)
        {
            btnUnequip.gameObject.SetActive(true);
            return;
        }

        // Món đồ này KHÔNG nằm trên người nhân vật hiện tại.
        // Ta phải kiểm tra xem nhân vật hiện tại có đang mặc cái gì ở vị trí (Part) này chưa?
        bool hasArmorInThisSlot = false;

        // Lấy data nhân vật từ SaveSystem
        var characterSave = playerCharacterManager.GetCharacter(characterID);
        if (characterSave != null && characterSave.SaveData.Armors != null)
        {
            foreach (var armorPart in characterSave.SaveData.Armors)
            {
                var eqArmor = inventoryManager.GetArmor(armorPart.ID);
                if (eqArmor != null)
                {
                    var eqConfig = gameDataBase.GetItemConfig(eqArmor.TemplateID);
                    if (eqConfig.Armor.Part == currentPart) // Đang mặc một món khác cùng Part (VD: Đang có Mũ)
                    {
                        hasArmorInThisSlot = true;
                        break;
                    }
                }
            }
        }

        if (hasArmorInThisSlot)
        {
            btnChange.gameObject.SetActive(true); // Đã có Mũ, bấm để Đổi Mũ khác
        }
        else
        {
            btnEquip.gameObject.SetActive(true);  // Đầu đang trần, bấm để Mặc Mũ
        }
    }
    private void ResetArmorStatsUI()
    {
        foreach (var obj in armorStats)
        {
            obj.gameObject.SetActive(false);
        }
    }

    private void UpdateArmorSubstatsUI(RolledSubStat stats)
    {
        foreach (var armor in armorStats)
        {
            if (armor.Type == stats.Type)
            {
                armor.gameObject.SetActive(true);
                armor.UpdateStat(stats.Value, stats.Level);
            }
        }
    }

}
