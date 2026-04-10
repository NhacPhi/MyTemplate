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
        btnChange.onClick.AddListener(() =>
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
        });

        btnUpgrade.onClick.AddListener(() =>
        {
            uiManager.ShowPanel(ScreenIds.UpgradeRelicPanel);
            UIEvent.OnSlelectWeaponEnchance?.Invoke(currentWeaponSeleted);
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
        UIEvent.OnCloseUpgradeRelicPanel += UpdateCurrentRelicInfo;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= UpdateWeaponInfo;
        UIEvent.OnCloseCharacterWeapon -= UpdateCharacterCardWeapon;
        UIEvent.OnCloseUpgradeRelicPanel -= UpdateCurrentRelicInfo;
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

                txtName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
                txtLevel.text = data.CurrentLevel.ToString();
                weaponConfig.Weapon.Upgrades.TryGetValue(StatType.HP, out int hpUpgradePerLevel);
                txtHP.text = (weaponConfig.Weapon.Stats.GetValueOrDefault(StatType.HP) + hpUpgradePerLevel * data.CurrentLevel).ToString();
                weaponConfig.Weapon.Upgrades.TryGetValue(StatType.ATK, out int atkUpgradePerLevel);
                txtATK.text = (weaponConfig.Weapon.Stats.GetValueOrDefault(StatType.ATK) + atkUpgradePerLevel * data.CurrentLevel).ToString();

                upgrades.UpdateUI(data.CurrentUpgrade);
                
                txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name) + " (Lv." + data.CurrentUpgrade + ")";
                txtSkill.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.UseDescription);
            }
            else
            {
                statInfo.gameObject.SetActive(false);
                weaponEmpty.gameObject.SetActive(true);
            }

            btnChange.gameObject.SetActive(true);
            btnEquip.gameObject.SetActive(false);
            btnUnEquip.gameObject.SetActive(false);
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


        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.CurrentLevel.ToString();

        config.Weapon.Upgrades.TryGetValue(StatType.HP, out int hpUpgradePerLevel);
        txtHP.text = (config.Weapon.Stats.GetValueOrDefault(StatType.HP) + hpUpgradePerLevel * data.CurrentLevel).ToString();
        config.Weapon.Upgrades.TryGetValue(StatType.ATK, out int atkUpgradePerLevel);
        txtATK.text = (config.Weapon.Stats.GetValueOrDefault(StatType.ATK) + atkUpgradePerLevel * data.CurrentLevel).ToString();

        txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
        txtSkill.text = LocalizationManager.Instance.GetLocalizedValue(config.UseDescription);
    }

    public void UpdateCharacterCardWeapon(bool close)
    {
        UpdateCharacterCardWeapon(currentCharacter);
        btnChange.gameObject.SetActive(true);
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
