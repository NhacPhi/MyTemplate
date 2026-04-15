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

    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateWeaponCardInfor;

        btnUpgrade.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.UpgradeRelicScene);
            UIEvent.OnSlelectWeaponEnchance?.Invoke(currentWeapon);
        });
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateWeaponCardInfor;
        btnUpgrade.onClick.RemoveAllListeners();
    }

    public void UpdateWeaponCardInfor(string uuid)
    {
        currentWeapon = uuid;   
        WeaponSaveData weapon = save.Player.Inventory.GetWeapon(uuid);
        var weaponConfig = gameDataBase.GetItemConfig(weapon.TemplateID);
        var passiveConfig = gameDataBase.GetPassiveConfig(weaponConfig.Weapon.PassiveID);
        
        if (weaponConfig != null)
        {
            txtWeaponName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
            txtLevel.text = LocalizationManager.Instance.GetLocalizedValue("UI_LEVEL") + "  " + weapon.CurrentLevel.ToString();

            txtHPNumber.text = weaponConfig.Weapon.GetStatByLevel(StatType.HP, weapon.CurrentLevel).ToString();
            txtATKNumber.text = weaponConfig.Weapon.GetStatByLevel(StatType.ATK, weapon.CurrentLevel).ToString();

            txtDes.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Description);
            txtSkillDes.text = passiveConfig.GetDescription(weapon.CurrentUpgrade);

            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtSkillDes.rectTransform);

            // Force rebuild UI layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
            weaponUI.Init(weapon.UUID, weaponConfig.Rarity, weaponConfig.Icon, weaponConfig.IconBG, weapon.CurrentLevel, weapon.CurrentUpgrade);
        }
    }    
}
