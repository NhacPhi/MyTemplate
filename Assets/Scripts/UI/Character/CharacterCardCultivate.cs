using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Collections.Generic;

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

    [SerializeField] private List<ItemUI> exps;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtCoinUpdateLv10;

    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardCultivate;
    }
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < exps.Count; i++)
        {
            ExpConfig expConfig = gameDataBase.ExpConfig[i];
            ItemBaseSO expSO = gameDataBase.GetItemSOByID<ItemBaseSO>(ItemType.Exp, expConfig.ID);
            ItemData itemData = save.Player.GetItem(expConfig.ID);
            exps[i].Init(expConfig.ID, expConfig.Rare, expSO.Icon, gameDataBase.GetRareBG(expConfig.Rare), itemData.Quantity);
            exps[i].CanClick = false;
        }

        UpdateCharacterCardCultivate(save.Player.GetIDOfFirstCharacter().ID);
    }


    private void OnDestroy()
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


        float currentHP = stat.HP + Utility.GetStatGrowthLevel(data.Level, upgrade.GrowthHP);
        float nextHP = stat.HP + Utility.GetStatGrowthLevel(data.Level + 1, upgrade.GrowthHP);

        float currentATK = stat.ATK + Utility.GetStatGrowthLevel(data.Level, upgrade.GrowthATK);
        float nextATK = stat.ATK + Utility.GetStatGrowthLevel(data.Level + 1, upgrade.GrowthATK);

        float currentDEF = stat.DEF + Utility.GetStatGrowthLevel(data.Level, upgrade.GrowthDEF);
        float nextDEF = stat.DEF + Utility.GetStatGrowthLevel(data.Level + 1, upgrade.GrowthDEF);

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

        txtCoin.text = Utility.GetCoinNeedToUpgradeCacultivate(data.Level + 1).ToString();
        txtCoinUpdateLv10.text = (Utility.GetCoinNeedToUpgradeCacultivate(10) - Utility.GetCoinNeedToUpgradeCacultivate(data.Level)).ToString();
    }

}
