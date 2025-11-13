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
    [SerializeField] private Button btnEquip;
    [SerializeField] private Button btnChange;
    [SerializeField] private Button btnUnEquip;
    [SerializeField] private Button btnUpgrade;

    [Inject] private IObjectResolver _objectResolver;
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
        _objectResolver.Inject(this);
        btnEquip.onClick.AddListener(() =>
        {
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke("");
        });

        btnChange.onClick.AddListener(() =>
        {
            UIEvent.OnSelectCharacterChangeWeapon?.Invoke(currentWeapon);
        });

        btnUpgrade.onClick.AddListener(() =>
        {
            uiManager.ShowPanel(ScreenIds.UpgradeRelicPanel);
            UIEvent.OnSlelectWeaponEnchance?.Invoke(currentWeapon);
        });
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
        currentCharacter = id;
        currentWeapon = save.Player.GetCharacter(id).Weapon;
        if(currentWeapon != "" )
        {
            statInfo.gameObject.SetActive(true);
            weaponEmpty.gameObject.SetActive(false);
            WeaponConfig config = gameDataBase.GetWeaponConfig(currentWeapon);
            WeaponData data = save.Player.GetWeapon(currentWeapon);
            WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, currentWeapon);

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            txtLevel.text = data.CurrentLevel.ToString();
            txtHP.text = config.HP.ToString();
            txtATK.text = config.ATK.ToString();

            txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
            txtSkill.text = LocalizationManager.Instance.GetLocalizedValue(config.SkillDes);
        }
        else
        {
            statInfo.gameObject.SetActive(false);
            weaponEmpty.gameObject.SetActive(true);
        }
    }

    public void UpdateWeaponInfo(string id)
    {
        statInfo.gameObject.SetActive(true);
        weaponEmpty.gameObject.SetActive(false);
        WeaponConfig config = gameDataBase.GetWeaponConfig(id);
        WeaponData data = save.Player.GetWeapon(id);
        WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, id);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.CurrentLevel.ToString();
        txtHP.text = config.HP.ToString();
        txtATK.text = config.ATK.ToString();

        txtUpgrade.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + " (Lv." + data.CurrentUpgrade + ")";
        txtSkill.text = LocalizationManager.Instance.GetLocalizedValue(config.SkillDes);
    }

    public void UpdateCharacterCardWeapon(bool close)
    {
        UpdateCharacterCardWeapon(currentCharacter);
    }
}
