using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TMPro;
using UnityEngine.UI;

public class CharacterCardRelic : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtATK;

    [SerializeField] private TextMeshProUGUI txtUpgrade;
    [SerializeField] private TextMeshProUGUI txtSkill;

    [SerializeField] private GameObject statInfo;
    [SerializeField] private GameObject weaponEmpty;

    //Button
    [SerializeField] private Button btnEquipWeapon;
    [SerializeField] private Button btnChange;
    [SerializeField] private Button btnUnEquip;
    [SerializeField] private Button btnEquip;
    [SerializeField] private Button btnUpgrade;

    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private UIManager uiManager;

    private string currentWeapon = "";
    private string originWeapon = "";
    private string currentCharacter = "";
    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardWeapon;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnEquipWeapon.onClick.AddListener(() =>
        {
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke("");
        });

        btnChange.onClick.AddListener(() =>
        {
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke(originWeapon);
            if (originWeapon != "")
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
            string weaponID = (originWeapon != currentWeapon && currentWeapon != null) ? currentWeapon : originWeapon;
            UIEvent.OnSlelectWeaponEnchance?.Invoke(weaponID);
        });

        UpdateCharacterCardWeapon(save.Player.GetIDOfFirstCharacter().ID);
    }

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponCard += UpdateWeaponInfo;
        UIEvent.OnCloseCharacterWeapon += UpdateCharacterCardWeapon;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponCard -= UpdateWeaponInfo;
        UIEvent.OnCloseCharacterWeapon -= UpdateCharacterCardWeapon;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardWeapon;
    }

    public void UpdateCharacterCardWeapon(string id)
    {
        if (id != "")
        {
            currentCharacter = id;
            originWeapon = save.Player.GetCharacter(id).Weapon;
            if (originWeapon != "")
            {
                statInfo.gameObject.SetActive(true);
                weaponEmpty.gameObject.SetActive(false);
                WeaponSaveData data = save.Player.GetWeapon(originWeapon);
                ItemConfig weaponConfig = gameDataBase.GetItemConfig(originWeapon);

                txtName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
                txtLevel.text = data.CurrentLevel.ToString();
                txtHP.text = weaponConfig.Weapon.Stats.GetValueOrDefault(StatType.HP).ToString();
                txtATK.text = weaponConfig.Weapon.Stats.GetValueOrDefault(StatType.ATK).ToString();

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

    public void UpdateWeaponInfo(string id)
    {
        if (originWeapon != "")
        {
            btnChange.gameObject.SetActive(true);
            btnUnEquip.gameObject.SetActive(false);
        }
        else
        {
            btnEquip.gameObject.SetActive(true);
        }
        btnChange.gameObject.SetActive(true);
        currentWeapon = id;
        if(originWeapon != null)
        {
            if (currentWeapon == originWeapon)
            {
                btnUnEquip.gameObject.SetActive(true);
            }
            else
            {
                btnChange.gameObject.SetActive(true);
            }
        }
        statInfo.gameObject.SetActive(true);
        weaponEmpty.gameObject.SetActive(false);
        ItemConfig config = gameDataBase.GetItemConfig(id);
        WeaponSaveData data = save.Player.GetWeapon(id);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.CurrentLevel.ToString();
        txtHP.text = config.Weapon.Stats.GetValueOrDefault(StatType.HP).ToString();
        txtATK.text = config.Weapon.Stats.GetValueOrDefault(StatType.ATK).ToString();

        txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
        txtSkill.text = LocalizationManager.Instance.GetLocalizedValue(config.UseDescription);
    }

    public void UpdateCharacterCardWeapon(bool close)
    {
        UpdateCharacterCardWeapon(currentCharacter);
        btnChange.gameObject.SetActive(true);
        currentWeapon = originWeapon;
    }
}
