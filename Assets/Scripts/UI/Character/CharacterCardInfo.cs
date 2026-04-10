using UnityEngine.UI;
using UnityEngine;
using TMPro;
using VContainer;
using System.Collections.Generic;
using Tech.Logger;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class CharacterCardInfo : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;
    [SerializeField]private UpgradesUI upgrades;

    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtATK;
    [SerializeField] private TextMeshProUGUI txtDEF;
    [SerializeField] private TextMeshProUGUI txtSPD;
    [SerializeField] private TextMeshProUGUI txtDEFShred;
    [SerializeField] private TextMeshProUGUI txtCritRate;
    [SerializeField] private TextMeshProUGUI txtCriteDMG;
    [SerializeField] private TextMeshProUGUI txtPenetration;
    [SerializeField] private TextMeshProUGUI txtCritDGMRes;

    [SerializeField] private Image imgAttack;
    [SerializeField] private Image imgMajorSkill;
    [SerializeField] private Image imgUltimateSkill;

    [Inject] PlayerCharacterManager characterManager;
    [Inject] private GameDataBase gameDataBase;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardInfo(characterManager.GetFirstCharacter().SaveData.ID);
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardInfo;
    }
    public void UpdateCharacterCardInfo(string id)
    {
        var characterProfile = characterManager.GetCharacter(id);
        CharacterConfig characterConfig = gameDataBase.GetCharacterConfig(id);
        if (characterManager == null)
        {
            LogCommon.Log("Character Data null with id: " + id);
            return;
        }

        upgrades.UpdateUI(characterProfile.SaveData.StarUp);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name);
        txtLevel.text = characterProfile.SaveData.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(characterConfig.Rare);

        txtHP.text = characterProfile.GetTotalStat(StatType.HP).ToString();
        txtATK.text = characterProfile.GetTotalStat(StatType.ATK).ToString();
        txtDEF.text = characterProfile.GetTotalStat(StatType.DEF).ToString(); // stat.DEF.ToString();
        txtSPD.text = characterProfile.GetTotalStat(StatType.SPEED).ToString();
        txtDEFShred.text = "0"; // stat.DEFShred.ToString();
        txtCritRate.text = "0"; // stat.CRITRate.ToString();
        txtCriteDMG.text = "0"; // stat.CRITDMG.ToString();
        txtPenetration.text = "0"; // stat.Penetration.ToString();
        txtCritDGMRes.text = "0"; // stat.CRITDMGRes.ToString();

        imgAttack.sprite = characterConfig.BaseSkillIcon;
        imgMajorSkill.sprite = characterConfig.MajorSkillIcon;
        imgUltimateSkill.sprite = characterConfig.UltimateSkillIcon;
    }
}
