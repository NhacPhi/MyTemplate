using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static Org.BouncyCastle.Math.EC.ECCurve;
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
        WeaponSaveData weapon = save.Player.GetWeapon(id);
        var weaponConfig = gameDataBase.GetItemConfig(id);
        
        if (weaponConfig != null)
        {
            txtWeaponName.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Name);
            txtLevel.text = LocalizationManager.Instance.GetLocalizedValue("UI_LEVEL") + "  " + weapon.CurrentLevel.ToString();
            weaponConfig.Weapon.Stats.TryGetValue(StatType.HP, out var hp);
            txtHPNumber.text = hp.ToString();
            weaponConfig.Weapon.Stats.TryGetValue(StatType.HP, out var atk);
            txtATKNumber.text = atk.ToString();
            txtDes.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.Description);
            txtSkillDes.text = LocalizationManager.Instance.GetLocalizedValue(weaponConfig.UseDescription);

            LayoutRebuilder.ForceRebuildLayoutImmediate(txtDes.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtSkillDes.rectTransform);

            // Force rebuild UI layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
            weaponUI.Init(weapon.ID, weaponConfig.Rarity, weaponConfig.Icon, weaponConfig.IconBG, weapon.CurrentLevel, weapon.CurrentUpgrade);
        }
    }    
}
