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
    [SerializeField] private UpgradesUI upgrades;

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

        upgrades.UpdateUI(data.StarUp);

        int expNeedToUpdate = gameDataBase.GetExpConfig(expTier).UpExp[(data.Level + 1).ToString()];
        txtCurrentExp.text = data.Exp.ToString() + "/" + expNeedToUpdate.ToString();

        sliderExp.maxValue = expNeedToUpdate;
        sliderExp.value = data.Exp;


        int currentHP = config.GetStatByLevel(StatType.HP, data.Level);
        float nextHP = config.GetStatByLevel(StatType.HP, data.Level + 1);

        float currentATK = config.GetStatByLevel(StatType.ATK, data.Level);
        float nextATK = config.GetStatByLevel(StatType.ATK, data.Level + 1);

        float currentDEF = config.GetStatByLevel(StatType.DEF, data.Level);
        float nextDEF = config.GetStatByLevel(StatType.DEF, data.Level + 1);

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
