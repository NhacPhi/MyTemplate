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

    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private PlayerCharacterManager playerCharacterManager;

    private readonly string[] EXP_ITEM_IDS = { "common_exp", "fine_exp", "rare_exp", "supreme_exp" };

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardCultivate;
    }
    // Start is called before the first frame update
    void Start()
    {
        RefrestUI();

        UpdateCharacterCardCultivate(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }


    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardCultivate;
    }

    public void RefrestUI()
    {

        for (int i = 0; i < EXP_ITEM_IDS.Length; i++)
        {
            string expItemID = EXP_ITEM_IDS[i];

            ItemUI slotUI;
            var expConfig = gameDataBase.GetItemConfig(expItemID);
            var expSave = inventory.GetItem(expItemID);
            var quantity = inventory.GetItemQuantity(expItemID);

            exps[i].Init(expSave.ID, expConfig.Rarity, expConfig.Icon, gameDataBase.GetBGItemByRare(expConfig.Rarity), quantity);
            exps[i].CanClick = false;
        }
    }

    public void UpdateCharacterCardCultivate(string id)
    {
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = playerCharacterManager.GetCharacter(id).SaveData;

        //CharacterUpgradeConfig upgrade = gameDataBase.GetCharacterUpgradeConfig(id);
        //CharacterStatConfig stat = gameDataBase.GetCharacterStatConfig(id);
        // Base info
        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);

        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(config.Rare);
        var expTier = Utility.GetExpConfigByCharacterRare(config.Rare);

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

        txtCoin.text = Utility.GetCoinNeedToUpgradeCacultivate(data.Level + 1).ToString();
        txtCoinUpdateLv10.text = (Utility.GetCoinNeedToUpgradeCacultivate(10) - Utility.GetCoinNeedToUpgradeCacultivate(data.Level)).ToString();
    }

}
