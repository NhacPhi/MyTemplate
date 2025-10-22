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

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private ItemDataBase itemData;
    [Inject] private SaveSystem save;

    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }

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
        Weapon weapon = save.GetWeapon(id);
        WeaponConfig config = itemData.GetWeaponConfig(id);
        WeaponSO weaponSO = itemData.GetWeaponSO(id);
        txtWeaponName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = LocalizationManager.Instance.GetLocalizedValue("UI_LEVEL") + "  "+ weapon.CurrentLevel.ToString();
        txtHPNumber.text = config.HP.ToString();
        txtATKNumber.text = config.Atk.ToString();
        txtDes.text = LocalizationManager.Instance.GetLocalizedValue(config.Description);
        txtSkillDes.text = LocalizationManager.Instance.GetLocalizedValue(config.SkillDes);

        LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(txtSkillDes.rectTransform);

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        weaponUI.Init(weapon.ID, config.Rare, weaponSO.Icon, itemData.GetRareBG(config.Rare), weapon.CurrentLevel, weapon.CurrentUpgrade);
    }    
}
