using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class WeaponCardInfoUI : MonoBehaviour
{
    [SerializeField] private WeaponUI weaponUI;
    [SerializeField] private TextMeshProUGUI txtWeaponName;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [SerializeField] private TextMeshProUGUI txtHPNumber;
    [SerializeField] private TextMeshProUGUI txtATKNumber;

    [SerializeField] private TextMeshProUGUI txtSkillDes;
    [SerializeField] private TextMeshProUGUI txtDes;

    [SerializeField] private GameObject content;

    [SerializeField] private Button btnUpgrade;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;
    [Inject] private UIManager uiManager;

    private string currentWeapon = "";

    private void Awake()
    {
        UIEvent.OnSelectInventoryItem += UpdateWeaponCardInfor;

        if (btnUpgrade != null)
        {
            btnUpgrade.onClick.AddListener(() =>
            {
                uiManager.OpenWindowScene(ScreenIds.UpgradeRelicScene);
                UIEvent.OnSlelectWeaponEnchance?.Invoke(currentWeapon);
            });
        }
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectInventoryItem -= UpdateWeaponCardInfor;
        if (btnUpgrade != null) btnUpgrade.onClick.RemoveAllListeners();
    }

    public void UpdateWeaponCardInfor(string uuid)
    {
        currentWeapon = uuid;   
        if (save == null || save.Player == null || save.Player.Inventory == null) return;
        WeaponSaveData weapon = save.Player.Inventory.GetWeapon(uuid);
        if (weapon == null || gameDataBase == null) return;
        var weaponConfig = gameDataBase.GetItemConfig(weapon.TemplateID);
        if (weaponConfig == null || weaponConfig.Weapon == null) return;
        
        var passiveConfig = gameDataBase.GetPassiveConfig(weaponConfig.Weapon.PassiveID);
        
        if (txtWeaponName != null) txtWeaponName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
        if (txtLevel != null) txtLevel.text = LocalizationManager.Instance.GetLocalizedValue("UI_LEVEL") + "  " + weapon.CurrentLevel.ToString();

        if (txtHPNumber != null) txtHPNumber.text = weaponConfig.Weapon.GetStatByLevel(StatType.HP, weapon.CurrentLevel).ToString();
        if (txtATKNumber != null) txtATKNumber.text = weaponConfig.Weapon.GetStatByLevel(StatType.ATK, weapon.CurrentLevel).ToString();

        if (txtDes != null) 
        {
            txtDes.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Description);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
        }

        if (txtSkillDes != null)
        {
            txtSkillDes.text = passiveConfig != null ? passiveConfig.GetDescription(weapon.CurrentUpgrade) : "";
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtSkillDes.rectTransform);
        }

        // Force rebuild UI layout
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }

        if (weaponUI != null)
        {
            bool isEquipped = !string.IsNullOrEmpty(weapon.Equip);
            bool isUnselectable = isEquipped || weaponConfig.Rarity == Rare.Legendary;
            weaponUI.Init(weapon.UUID, weaponConfig.Rarity, weaponConfig.Icon, weaponConfig.IconBG, weapon.CurrentLevel, weapon.CurrentUpgrade, isUnselectable);
        }
    }    
}
