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
        CharacterConfig characterConfig = gameDataBase.GetCharacterConfig(id);
        if (characterManager == null)
        {
            LogCommon.Log("Character Data null with id: " + id);
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
        txtDEFShred.text = "0"; // stat.DEFShred.ToString();
        txtCritRate.text = GetStatText(StatType.CRIT_RATE, characterProfile);
        txtCriteDMG.text = GetStatText(StatType.CRIT_DMG, characterProfile);
        txtPenetration.text = "0"; // stat.Penetration.ToString();
        txtCritDGMRes.text = "0"; // stat.CRITDMGRes.ToString();

        int starUp = characterProfile.SaveData.StarUp;
        baseSkill.SetSkillUI(characterConfig.BaseSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Base, starUp));
        mainSkill.SetSkillUI(characterConfig.MajorSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Major, starUp));
        ultimateSkill.SetSkillUI(characterConfig.UltimateSkillIcon, 
            Utility.GetSkillEnhancementLevel(SkillCharacter.Ultimate, starUp));

        // Truyền character context cho tooltip handler
        baseSkill.SetCharacterID(id);
        mainSkill.SetCharacterID(id);
        ultimateSkill.SetCharacterID(id);
    }

    public void UpdateCardInfoWithCurrentCharacter()
    {
        UpdateCharacterCardInfo(currentCharracter);
    }

    private string GetStatText(StatType type, CharacterProfileModel profile)
    {
        int profileTotal = profile.GetTotalStat(type);
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
                    if (buff.ModifierType == ModifyType.Constant) globalFlat += buff.Value;
                    else if (buff.ModifierType == ModifyType.Percent) globalPercent += buff.Value;
                }
            }
        }

        if (globalFlat == 0f && globalPercent == 0f)
        {
            return profileTotal.ToString();
        }

        int foodBonus = Mathf.RoundToInt(globalFlat + (profileTotal * (globalPercent / 100f)));
        Debug.Log($"[CharacterCardInfo] Stat: {type}, ProfileTotal: {profileTotal}, FoodBonus: {foodBonus} (Flat: {globalFlat}, Percent: {globalPercent})");

        if (foodBonus > 0)
        {
            return $"{profileTotal} <color=#00FF00>+{foodBonus}</color>";
        }
        else if (foodBonus < 0)
        {
            return $"{profileTotal} <color=#FF0000>{foodBonus}</color>";
        }
        return profileTotal.ToString();
    }
}
