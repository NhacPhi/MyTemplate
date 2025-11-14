using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;
public class WeaponAscendCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;

    [SerializeField] private TextMeshProUGUI txtCurentHP;
    [SerializeField] private TextMeshProUGUI txtCurentATK;

    [SerializeField] private TextMeshProUGUI txtNameAndLevel;
    [SerializeField] private TextMeshProUGUI txtUse;
    [SerializeField] private TextMeshProUGUI txtCoin;

    [Inject] IObjectResolver _resolver;
    [Inject] GameDataBase gameDataBase;
    [Inject] SaveSystem save;

    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdateWeaponAscendCard;
    }
    // Start is called before the first frame update
    void Start()
    {
        _resolver.Inject(this);
    }

    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdateWeaponAscendCard;
    }

    public void UpdateWeaponAscendCard(string weaponID)
    {
        if (weaponID != "")
        {
            WeaponConfig config = gameDataBase.GetWeaponConfig(weaponID);
            WeaponData data = save.Player.GetWeapon(weaponID);
            WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, weaponID);

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = data.CurrentLevel;
            txtLevel.text = level.ToString() + "/10";

            int currentHP = config.HP + Utility.GetStatGrowthLevel(level, config.GrowthHP);
            int currentATK = config.ATK + Utility.GetStatGrowthLevel(level + 1, config.GrowthATK);

            txtCurentHP.text = currentHP.ToString();
            txtCurentATK.text = currentATK.ToString();

            txtNameAndLevel.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + "(Lv." + data.CurrentUpgrade.ToString() + ")";
            txtUse.text = LocalizationManager.Instance.GetLocalizedValue(config.SkillDes);
            txtCoin.text = Utility.GetCoinNeedToAsscendWeapon(data.CurrentUpgrade).ToString();
        }
    }
}
