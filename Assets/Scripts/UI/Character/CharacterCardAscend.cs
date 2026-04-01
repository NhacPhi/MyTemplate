using UnityEngine.UI;
using TMPro;
using UnityEngine;
using VContainer;

public class CharacterCardAscend : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;

    [SerializeField] private Image iconShard;
    [SerializeField] private TextMeshProUGUI txtNumberShard;
    [SerializeField] private TextMeshProUGUI txtCoin;

    [Inject] private PlayerCharacterManager playerCharacterManager;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private GameDataBase gameDataBase;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardAscend;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardAscend;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardAscend(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }

    public void UpdateCharacterCardAscend(string id)
    {
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = playerCharacterManager.GetCharacter(id).SaveData;

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(config.Rare);

        iconShard.sprite = config.Icon;
        txtNumberShard.text = inventoryManager.GetItem(id).Quantity.ToString() + "/" + Utility.GetShardNeedToUpgradeAscend(data.BoostStat + 1);

        txtCoin.text = Utility.GetCoinNeedToAscendCharacter(data.BoostStat + 1).ToString();
    }
}
