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
            string targetWeapon = string.IsNullOrEmpty(currentWeaponSeleted) ? weaponOfCharacter : currentWeaponSeleted;
            UIEvent.OnSlelectWeaponEnchance?.Invoke(targetWeapon);
            UIEvent.OnSelectWeaponEnchanceFromCharacter?.Invoke(currentCharacter);
        });

        btnEquip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            if (character != null)
            {
                character.EquipWeapon(currentWeaponSeleted);
                UIEvent.OnUpdateSingleWeaponCard?.Invoke(currentWeaponSeleted);
                weaponOfCharacter = currentWeaponSeleted;
                UpdateWeaponInfo(currentWeaponSeleted);
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
        });

        btnUnEquip.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            if (character != null)
            {
                character.UnEquipWeapon(currentWeaponSeleted);
                UIEvent.OnUpdateSingleWeaponCard?.Invoke(currentWeaponSeleted);
                weaponOfCharacter = "";
                UpdateWeaponInfo(currentWeaponSeleted);
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
        });

        btnChange.onClick.AddListener(() =>
        {
            var character = playerCharacterManager.GetCharacter(currentCharacter);
            if (character != null)
            {
                character.ChangeWeapon(currentWeaponSeleted);
                UIEvent.OnUpdateSingleWeaponCard?.Invoke(currentWeaponSeleted);
                UIEvent.OnUpdateSingleWeaponCard?.Invoke(weaponOfCharacter);
                weaponOfCharacter = currentWeaponSeleted;
                UpdateWeaponInfo(currentWeaponSeleted);
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
        });

        string id = playerCharacterManager.CurrentSelectedCharacterID;
        var firstChar = playerCharacterManager.GetFirstCharacter();
        if (string.IsNullOrEmpty(id) && firstChar != null && firstChar.SaveData != null) id = firstChar.SaveData.ID;
        UpdateCharacterCardWeapon(id);
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
        if (!string.IsNullOrEmpty(characterid))
        {
            currentCharacter = characterid;
            var character = playerCharacterManager.GetCharacter(characterid);
            if (character == null || character.SaveData == null)
            {
                statInfo.gameObject.SetActive(false);
                weaponEmpty.gameObject.SetActive(true);
                return;
            }

            var characterSave = character.SaveData;
            weaponOfCharacter = characterSave.Weapon;
            currentWeaponSeleted = weaponOfCharacter;

            if (!string.IsNullOrEmpty(weaponOfCharacter))
            {
                WeaponSaveData data = inventoryManager.GetWeapon(weaponOfCharacter);
                if (data != null)
                {
                    ItemConfig weaponConfig = gameDataBase.GetItemConfig(data.TemplateID);
                    if (weaponConfig != null && weaponConfig.Weapon != null)
                    {
                        statInfo.gameObject.SetActive(true);
                        weaponEmpty.gameObject.SetActive(false);

                        var passiveConfig = gameDataBase.GetPassiveConfig(weaponConfig.Weapon.PassiveID);

                        if (txtName != null) txtName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
                        if (txtLevel != null) txtLevel.text = data.CurrentLevel.ToString();

                        if (txtHP != null) txtHP.text = weaponConfig.Weapon.GetStatByLevel(StatType.HP, data.CurrentLevel).ToString();
                        if (txtATK != null) txtATK.text = weaponConfig.Weapon.GetStatByLevel(StatType.ATK, data.CurrentLevel).ToString();

                        if (upgrades != null) upgrades.UpdateUI(data.CurrentUpgrade);

                        if (txtUpgrade != null) txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name) + " (Lv." + data.CurrentUpgrade + ")";
                        if (txtSkill != null) txtSkill.text = passiveConfig != null ? passiveConfig.GetDescription(data.CurrentUpgrade) : "";
                        return;
                    }
                }
            }

            statInfo.gameObject.SetActive(false);
            weaponEmpty.gameObject.SetActive(true);
        }
    }

    public void UpdateWeaponInfo(string uuid)
    {
        if (string.IsNullOrEmpty(uuid))
        {
            statInfo.gameObject.SetActive(false);
            weaponEmpty.gameObject.SetActive(true);
            return;
        }

        if (weaponOfCharacter != "")
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

        WeaponSaveData data = inventoryManager.GetWeapon(uuid);
        if (data == null)
        {
            statInfo.gameObject.SetActive(false);
            weaponEmpty.gameObject.SetActive(true);
            return;
        }

        ItemConfig config = gameDataBase.GetItemConfig(data.TemplateID);
        if (config == null || config.Weapon == null)
        {
            statInfo.gameObject.SetActive(false);
            weaponEmpty.gameObject.SetActive(true);
            return;
        }

        statInfo.gameObject.SetActive(true);
        weaponEmpty.gameObject.SetActive(false);

        var passiveConfig = gameDataBase.GetPassiveConfig(config.Weapon.PassiveID);

        if (txtName != null) txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        if (txtLevel != null) txtLevel.text = data.CurrentLevel.ToString();

        config.Weapon.Upgrades.TryGetValue(StatType.HP, out int hpUpgradePerLevel);
        if (txtHP != null) txtHP.text = (config.Weapon.Stats.GetValueOrDefault(StatType.HP) + hpUpgradePerLevel * data.CurrentLevel).ToString();
        config.Weapon.Upgrades.TryGetValue(StatType.ATK, out int atkUpgradePerLevel);
        if (txtATK != null) txtATK.text = (config.Weapon.Stats.GetValueOrDefault(StatType.ATK) + atkUpgradePerLevel * data.CurrentLevel).ToString();

        if (txtUpgrade != null) txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
        if (txtSkill != null) txtSkill.text = passiveConfig != null ? passiveConfig.GetDescription(data.CurrentUpgrade) : "";
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
