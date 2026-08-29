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

    [SerializeField] private SkillCharacterUI baseSkill;
    [SerializeField] private SkillCharacterUI mainSkill;
    [SerializeField] private SkillCharacterUI ultimateSkill;

    [Inject] PlayerCharacterManager characterManager;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem saveSystem;

    private string currentCharracter = "";

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardInfo;
        UIEvent.OnCloseUpgradeRelicScene += UpdateCardInfoWithCurrentCharacter;
        UIEvent.OnCloseUpgradeArmorScene += UpdateCardInfoWithCurrentCharacter;
    }

    // Start is called before the first frame update
    void Start()
    {
        string id = characterManager.CurrentSelectedCharacterID;
        if (string.IsNullOrEmpty(id)) id = characterManager.GetFirstCharacter().SaveData.ID;
        UpdateCharacterCardInfo(id);
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardInfo;
        UIEvent.OnCloseUpgradeRelicScene -= UpdateCardInfoWithCurrentCharacter;
        UIEvent.OnCloseUpgradeArmorScene -= UpdateCardInfoWithCurrentCharacter;
    }
    public void UpdateCharacterCardInfo(string id)
    {
        currentCharracter = id;
        var characterProfile = characterManager.GetCharacter(id);
        if (characterProfile != null)
        {
            characterProfile.RefreshAllEquippedArmors();
        }

        CharacterConfig characterConfig = gameDataBase.GetCharacterConfig(id);
        if (characterConfig == null)
        {
            LogCommon.Log("Character config null with id: " + id);
            return;
        }

        if (characterProfile == null)
        {
            if (upgrades != null) upgrades.UpdateUI(0);
            if (txtName != null) txtName.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name) : characterConfig.Name.ToString();
            if (txtLevel != null) txtLevel.text = "1/" + Definition.MAX_CHARACTER_LEVEL.ToString();
            if (iconRare != null) iconRare.sprite = gameDataBase.GetCharacterRareIcon(characterConfig.Rare);

            if (txtHP != null) txtHP.text = characterConfig.GetStat(StatType.HP).ToString();
            if (txtATK != null) txtATK.text = characterConfig.GetStat(StatType.ATK).ToString();
            if (txtDEF != null) txtDEF.text = characterConfig.GetStat(StatType.DEF).ToString();
            if (txtSPD != null) txtSPD.text = characterConfig.GetStat(StatType.SPEED).ToString();
            if (txtDEFShred != null) txtDEFShred.text = characterConfig.GetStat(StatType.DEF_SHRED).ToString();
            if (txtCritRate != null) txtCritRate.text = characterConfig.GetStat(StatType.CRIT_RATE).ToString() + "%";
            if (txtCriteDMG != null) txtCriteDMG.text = (characterConfig.GetStat(StatType.CRIT_DMG) + 175).ToString() + "%";
            if (txtPenetration != null) txtPenetration.text = characterConfig.GetStat(StatType.PENETRATION).ToString() + "%";
            if (txtCritDGMRes != null) txtCritDGMRes.text = characterConfig.GetStat(StatType.CRIT_DMG_RES).ToString() + "%";

            if (baseSkill != null) baseSkill.SetSkillUI(characterConfig.BaseSkillIcon, 1);
            if (mainSkill != null) mainSkill.SetSkillUI(characterConfig.MajorSkillIcon, 1);
            if (ultimateSkill != null) ultimateSkill.SetSkillUI(characterConfig.UltimateSkillIcon, 1);

            if (baseSkill != null) baseSkill.SetCharacterID(id, SkillCharacter.Base);
            if (mainSkill != null) mainSkill.SetCharacterID(id, SkillCharacter.Major);
            if (ultimateSkill != null) ultimateSkill.SetCharacterID(id, SkillCharacter.Ultimate);
            return;
        }

        upgrades.UpdateUI(characterProfile.SaveData.StarUp);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name);
        txtLevel.text = characterProfile.SaveData.Level.ToString() + "/" + Definition.MAX_CHARACTER_LEVEL.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(characterConfig.Rare);

        txtHP.text = GetStatText(StatType.HP, characterProfile);
        txtATK.text = GetStatText(StatType.ATK, characterProfile);
        txtDEF.text = GetStatText(StatType.DEF, characterProfile);
        txtSPD.text = GetStatText(StatType.SPEED, characterProfile);
        txtDEFShred.text = GetStatText(StatType.DEF_SHRED, characterProfile);
        txtCritRate.text = GetStatText(StatType.CRIT_RATE, characterProfile, true);
        txtCriteDMG.text = GetStatText(StatType.CRIT_DMG, characterProfile, true, 175);
        txtPenetration.text = GetStatText(StatType.PENETRATION, characterProfile, true);
        txtCritDGMRes.text = GetStatText(StatType.CRIT_DMG_RES, characterProfile, true);

        int starUp = characterProfile.SaveData.StarUp;
        baseSkill.SetSkillUI(characterConfig.BaseSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Base, starUp));
        mainSkill.SetSkillUI(characterConfig.MajorSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Major, starUp));
        ultimateSkill.SetSkillUI(characterConfig.UltimateSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Ultimate, starUp));

        // Truyền character context cho tooltip handler
        baseSkill.SetCharacterID(id, SkillCharacter.Base);
        mainSkill.SetCharacterID(id, SkillCharacter.Major);
        ultimateSkill.SetCharacterID(id, SkillCharacter.Ultimate);
    }

    public void UpdateCardInfoWithCurrentCharacter()
    {
        UpdateCharacterCardInfo(currentCharracter);
    }

    private string GetStatText(StatType type, CharacterProfileModel profile, bool isPercentage = false, int baseOffset = 0)
    {
        int originalProfileTotal = profile.GetTotalStat(type);
        int profileTotal = originalProfileTotal + baseOffset;

        float globalFlat = 0f;
        float globalPercent = 0f;

        if (saveSystem == null)
        {
            Debug.LogWarning("[CharacterCardInfo] SaveSystem is null! Injection failed or hot-reload issue.");
        }
        else if (saveSystem.Player?.Roster?.ActiveGlobalBuffs != null)
        {
            foreach (var buff in saveSystem.Player.Roster.ActiveGlobalBuffs)
            {
                if (buff.IsActive && buff.StatType == type)
                {
                    if (buff.ModifierType == ModifyType.Flat) globalFlat += buff.Value;
                    else if (buff.ModifierType == ModifyType.Percent) globalPercent += buff.Value;
                }
            }
        }

        string pct = isPercentage ? "%" : "";

        if (type == StatType.CRIT_RATE && profileTotal > 100)
        {
            profileTotal = 100;
        }

        if (globalFlat == 0f && globalPercent == 0f)
        {
            return $"{profileTotal}{pct}";
        }

        int foodBonus = Mathf.RoundToInt(globalFlat + (originalProfileTotal * (globalPercent / 100f)));

        if (type == StatType.CRIT_RATE && profileTotal + foodBonus > 100)
        {
            foodBonus = 100 - profileTotal;
        }

        if (foodBonus > 0)
        {
            return $"{profileTotal}{pct} <color=#00FF00>+{foodBonus}{pct}</color>";
        }
        else if (foodBonus < 0)
        {
            return $"{profileTotal}{pct} <color=#FF0000>{foodBonus}{pct}</color>";
        }

        return $"{profileTotal}{pct}";
    }
}
