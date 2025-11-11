using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TMPro;
using Org.BouncyCastle.Asn1.Tsp;

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

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardWeapon;
    }
    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardWeapon;
    }

    public void UpdateCharacterCardWeapon(string id)
    {
        string weaponID = save.Player.GetCharacter(id).Weapon;
        if(weaponID != "" )
        {
            statInfo.gameObject.SetActive(true);
            weaponEmpty.gameObject.SetActive(false);
            WeaponConfig config = gameDataBase.GetWeaponConfig(weaponID);
            WeaponData data = save.Player.GetWeapon(weaponID);
            WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, weaponID);

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
}
