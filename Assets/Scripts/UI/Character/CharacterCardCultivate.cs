using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Collections.Generic;
using static SixLabors.ImageSharp.Metadata.Profiles.Exif.EncodedString;

public class CharacterCardCultivate : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;
    [SerializeField] private UpgradeUI[] upgrades;

    [SerializeField] private TextMeshProUGUI txtCurrentExp;
    [SerializeField] private Slider sliderExp;

    [SerializeField] private TextMeshProUGUI txtCurrentHP;
    [SerializeField] private TextMeshProUGUI txtCurrentATK;
    [SerializeField] private TextMeshProUGUI txtCurrentDEF;

    [SerializeField] private TextMeshProUGUI txtNextHP;
    [SerializeField] private TextMeshProUGUI txtNextATK;
    [SerializeField] private TextMeshProUGUI txtNextDEF;

    [SerializeField] private CharacterUpdateLevel UpdateLevelHub;
    [SerializeField] private CharacterAscentionUpgrade AscentionHub;

    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private PlayerCharacterManager playerCharacterManager;

    private string currentCharacter = string.Empty;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardCultivate;
    }

    void Start()
    {
        UpdateLevelHub.RefreshUI();

        UpdateCharacterCardCultivate(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }


    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardCultivate;
    }


    public void UpdateCharacterCardCultivate(string id)
    {
        currentCharacter = id;
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = playerCharacterManager.GetCharacter(id).SaveData;

        // Base info
        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);

        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(config.Rare);
        var expTier = Utility.GetExpConfigIDByCharacterRare(config.Rare);

        for (int i = 0; i < upgrades.Length; i++)
        {
            if (i < data.StarUp)
            {
                upgrades[i].ActiveLayer(1);
            }
            else
            {
                upgrades[i].ActiveLayer(0);
            }
        }

        int expNeedToUpdate = gameDataBase.GetExpConfig(expTier).UpExp[(data.Level + 1).ToString()];
        txtCurrentExp.text = data.Exp.ToString() + "/" + expNeedToUpdate.ToString();

        sliderExp.maxValue = expNeedToUpdate;
        sliderExp.value = data.Exp;


        int currentHP = config.GetStat(StatType.HP) + Utility.GetStatGrowthLevel(data.Level, config.GetUpdateStat(StatType.HP));
        float nextHP = config.GetStat(StatType.HP) + Utility.GetStatGrowthLevel(data.Level + 1, config.GetUpdateStat(StatType.HP));

        float currentATK = config.GetStat(StatType.ATK) + Utility.GetStatGrowthLevel(data.Level, config.GetUpdateStat(StatType.ATK));
        float nextATK = config.GetStat(StatType.ATK) + Utility.GetStatGrowthLevel(data.Level + 1, config.GetUpdateStat(StatType.ATK));

        float currentDEF = config.GetStat(StatType.DEF) + Utility.GetStatGrowthLevel(data.Level, config.GetUpdateStat(StatType.DEF));
        float nextDEF = config.GetStat(StatType.DEF) + Utility.GetStatGrowthLevel(data.Level + 1, config.GetUpdateStat(StatType.DEF));

        txtCurrentHP.text = currentHP.ToString();
        txtCurrentATK.text = currentATK.ToString();
        txtCurrentDEF.text = currentDEF.ToString();

        if (data.Level < 100)
        {
            txtNextHP.text = nextHP.ToString();
            txtNextATK.text = nextATK.ToString();
            txtNextDEF.text = nextDEF.ToString();
        }
        else
        {
            txtNextHP.text = txtNextATK.text = txtNextDEF.text = LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");
        }

        if (AscentionHub.IsShowCharactterAscentionUpgrade(id))
        {
            UpdateLevelHub.gameObject.SetActive(false);
            AscentionHub.gameObject.SetActive(true);

            AscentionHub.CharacterAscentionUpdate(id);
        }
        else
        {
            UpdateLevelHub.gameObject.SetActive(true);
            AscentionHub.gameObject.SetActive(false);

            UpdateLevelHub.UpdateCharacterUpdateLevel(id);
        }

    }

}
