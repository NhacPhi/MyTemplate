using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CharacterCardCultivate : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;

    [SerializeField] private TextMeshProUGUI txtCurrentExp;
    [SerializeField] private Slider sliderExp;

    [SerializeField] private TextMeshProUGUI txtCurrentHP;
    [SerializeField] private TextMeshProUGUI txtCurrentATK;
    [SerializeField] private TextMeshProUGUI txtCurrentDEF;

    [SerializeField] private TextMeshProUGUI txtNextHP;
    [SerializeField] private TextMeshProUGUI txtNextATK;
    [SerializeField] private TextMeshProUGUI txtNextDEF;

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;
    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }

    private void OnEnable()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardCultivate;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardCultivate;
    }

    public void UpdateCharacterCardCultivate(string id)
    {
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterData data = save.Player.GetCharacter(id);
        CharacterUpgradeConfig upgrade = gameDataBase.GetCharacterUpgradeConfig(id);
        CharacterStatConfig stat = gameDataBase.GetCharacterStatConfig(id);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();
        iconRare.sprite = gameDataBase.GetCharacterRareSO(config.Rare).Icon;

        int ExpConfig = Utility.GetCharacterExpByLevel(data.Level);
        txtCurrentExp.text = data.Exp.ToString() + "/" + ExpConfig.ToString();

        sliderExp.maxValue = ExpConfig;
        sliderExp.value = data.Exp;


        float currentHP = stat.HP + Utility.GetCharacterStatGrowthLevel(data.Level, upgrade.GrowthHP);
        float nextHP = stat.HP + Utility.GetCharacterStatGrowthLevel(data.Level + 1, upgrade.GrowthHP);

        float currentATK = stat.ATK + Utility.GetCharacterStatGrowthLevel(data.Level, upgrade.GrowthATK);
        float nextATK = stat.ATK + Utility.GetCharacterStatGrowthLevel(data.Level + 1, upgrade.GrowthATK);

        float currentDEF = stat.DEF + Utility.GetCharacterStatGrowthLevel(data.Level, upgrade.GrowthDEF);
        float nextDEF = stat.DEF + Utility.GetCharacterStatGrowthLevel(data.Level + 1, upgrade.GrowthDEF);

        txtCurrentHP.text = currentHP.ToString();
        txtCurrentATK.text = currentATK.ToString();
        txtCurrentDEF.text = currentDEF.ToString();

        if (data.Level < 10)
        {
            txtNextHP.text = nextHP.ToString();
            txtNextATK.text = nextATK.ToString();
            txtNextDEF.text = nextDEF.ToString();
        }
        else
        {
            txtNextHP.text = txtNextATK.text = txtNextDEF.text = LocalizationManager.Instance.GetLocalizedValue("STR_MAX_LEVEL");
        }

    }

}
