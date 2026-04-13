using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TMPro;
using UnityEngine.UI;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class CharacterCardRelic : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgrades;

    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtATK;

    [SerializeField] private TextMeshProUGUI txtUpgrade;
    [SerializeField] private TextMeshProUGUI txtSkill;

    [SerializeField] private GameObject statInfo;
    [SerializeField] private GameObject weaponEmpty;

    //Button
    [SerializeField] private Button btnChange;
    [SerializeField] private Button btnOpenWeaponCategory;
    [SerializeField] private Button btnUnEquip;
    [SerializeField] private Button btnEquip;
    [SerializeField] private Button btnUpgrade;

    [Inject] private PlayerCharacterManager playerCharacterManager;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private UIManager uiManager;

    private string currentWeaponSeleted = "";
    private string weaponOfCharacter = "";
    private string currentCharacter = "";
    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardWeapon;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnOpenWeaponCategory.onClick.AddListener(() =>
        {
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke(weaponOfCharacter);

            if (weaponOfCharacter != "")
            {
                btnUnEquip.gameObject.SetActive(true);
            }
            else
            {
                btnEquip.gameObject.SetActive(true);
                btnUnEquip.gameObject.SetActive(false);
            }

            btnOpenWeaponCategory.gameObject.SetActive(false);
        });

        btnUpgrade.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.UpgradeRelicScene);
            UIEvent.OnSlelectWeaponEnchance?.Invoke(weaponOfCharacter);
            UIEvent.OnSelectWeaponEnchanceFromCharacter.Invoke(currentCharacter);
        });

        btnEquip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            character.EquipWeapon(currentWeaponSeleted);
            UIEvent.OnUpdateSingleWeaponCard(currentWeaponSeleted);
            weaponOfCharacter = currentWeaponSeleted;
            UpdateWeaponInfo(currentWeaponSeleted);

            UIEvent.OnSelectCharacterAvatar(currentCharacter);
        });

        btnUnEquip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            character.UnEquipWeapon(currentWeaponSeleted);
            UIEvent.OnUpdateSingleWeaponCard(currentWeaponSeleted);
            weaponOfCharacter = "";
            UpdateWeaponInfo(currentWeaponSeleted);

            UIEvent.OnSelectCharacterAvatar(currentCharacter);
        });

        btnChange.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            character.ChangeWeapon(currentWeaponSeleted);
            UIEvent.OnUpdateSingleWeaponCard(currentWeaponSeleted);
            UIEvent.OnUpdateSingleWeaponCard(weaponOfCharacter);
            weaponOfCharacter = currentWeaponSeleted;
            UpdateWeaponInfo(currentWeaponSeleted);

            UIEvent.OnSelectCharacterAvatar(currentCharacter);
        });

        UpdateCharacterCardWeapon(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += UpdateWeaponInfo;
        UIEvent.OnCloseCharacterWeapon += UpdateCharacterCardWeapon;
        UIEvent.OnCloseUpgradeRelicScene += UpdateCurrentRelicInfo;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= UpdateWeaponInfo;
        UIEvent.OnCloseCharacterWeapon -= UpdateCharacterCardWeapon;
        UIEvent.OnCloseUpgradeRelicScene -= UpdateCurrentRelicInfo;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardWeapon;
    }

    public void UpdateCharacterCardWeapon(string characterid)
    {
        if (characterid != "")
        {
            currentCharacter = characterid;
            var characterSave = playerCharacterManager.GetCharacter(characterid).SaveData;
            weaponOfCharacter = characterSave.Weapon;
            if (weaponOfCharacter != "")
            {
                statInfo.gameObject.SetActive(true);
                weaponEmpty.gameObject.SetActive(false);
                WeaponSaveData data = inventoryManager.GetWeapon(weaponOfCharacter);
                ItemConfig weaponConfig = gameDataBase.GetItemConfig(data.TemplateID);
                var passiveConfig = gameDataBase.GetPassiveConfig(weaponConfig.Weapon.PassiveID);

                txtName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
                txtLevel.text = data.CurrentLevel.ToString();

                txtHP.text = weaponConfig.Weapon.GetStatByLevel(StatType.HP, data.CurrentLevel).ToString();

                txtATK.text = weaponConfig.Weapon.GetStatByLevel(StatType.ATK, data.CurrentLevel).ToString();

                upgrades.UpdateUI(data.CurrentUpgrade);
                
                txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name) + " (Lv." + data.CurrentUpgrade + ")";

                txtSkill.text = passiveConfig.GetDescription(data.CurrentUpgrade);
            }
            else
            {
                statInfo.gameObject.SetActive(false);
                weaponEmpty.gameObject.SetActive(true);
            }

        }
    }

    public void UpdateWeaponInfo(string uuid)
    {
        if(weaponOfCharacter != "")
        {
            if (uuid == weaponOfCharacter)
            {
                btnUnEquip.gameObject.SetActive(true);

                btnChange.gameObject.SetActive(false);
                btnEquip.gameObject.SetActive(true);
            }
            else
            {
                btnChange.gameObject.SetActive(true);

                btnEquip.gameObject.SetActive(false);
                btnUnEquip.gameObject.SetActive(false);
            }

            
        }
        else
        {
            btnEquip.gameObject.SetActive(true);

            btnUnEquip.gameObject.SetActive(false);
            btnChange.gameObject.SetActive(false);
        }


        currentWeaponSeleted = uuid;
        statInfo.gameObject.SetActive(true);
        weaponEmpty.gameObject.SetActive(false);
        WeaponSaveData data = inventoryManager.GetWeapon(uuid);
        ItemConfig config = gameDataBase.GetItemConfig(data.TemplateID);
        var passiveConfig = gameDataBase.GetPassiveConfig(config.Weapon.PassiveID);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.CurrentLevel.ToString();

        config.Weapon.Upgrades.TryGetValue(StatType.HP, out int hpUpgradePerLevel);
        txtHP.text = (config.Weapon.Stats.GetValueOrDefault(StatType.HP) + hpUpgradePerLevel * data.CurrentLevel).ToString();
        config.Weapon.Upgrades.TryGetValue(StatType.ATK, out int atkUpgradePerLevel);
        txtATK.text = (config.Weapon.Stats.GetValueOrDefault(StatType.ATK) + atkUpgradePerLevel * data.CurrentLevel).ToString();

        txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
        txtSkill.text = passiveConfig.GetDescription(data.CurrentUpgrade);
    }

    public void UpdateCharacterCardWeapon(bool close)
    {
        UpdateCharacterCardWeapon(currentCharacter);
        btnOpenWeaponCategory.gameObject.SetActive(true);
        btnEquip.gameObject.SetActive(false);
        btnUnEquip.gameObject.SetActive(false);
        btnChange.gameObject.SetActive(false);
        currentWeaponSeleted = weaponOfCharacter;
    }

    private void UpdateCurrentRelicInfo()
    {
        if (!string.IsNullOrEmpty(currentWeaponSeleted))
        {
            UpdateWeaponInfo(currentWeaponSeleted);
        }
    }
}
