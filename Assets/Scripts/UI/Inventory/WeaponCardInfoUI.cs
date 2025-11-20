using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static UnityEditor.Progress;

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

    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;


    private void OnEnable()
    {
        UIEvent.OnSelectInventoryItem += UpdateWeaponCardInfor;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectInventoryItem -= UpdateWeaponCardInfor;
    }

    public void UpdateWeaponCardInfor(string id)
    {
        WeaponData weapon = save.Player.GetWeapon(id);
        WeaponConfig config = gameDataBase.GetItemConfigByID<WeaponConfig>(ItemType.Weapon, id);
        WeaponSO weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, id);
        txtWeaponName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = LocalizationManager.Instance.GetLocalizedValue("UI_LEVEL") + "  "+ weapon.CurrentLevel.ToString();
        txtHPNumber.text = config.HP.ToString();
        txtATKNumber.text = config.ATK.ToString();
        txtDes.text = LocalizationManager.Instance.GetLocalizedValue(config.Description);
        txtSkillDes.text = LocalizationManager.Instance.GetLocalizedValue(config.SkillDes);

        LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(txtSkillDes.rectTransform);

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        weaponUI.Init(weapon.ID, config.Rare, weaponSO.Icon, gameDataBase.GetRareBG(config.Rare), weapon.CurrentLevel, weapon.CurrentUpgrade);
    }    
}
