using UnityEngine.UI;
using TMPro;
using UnityEngine;
using VContainer;

public class CharacterCardAscend : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;
    [SerializeField] private UpgradeUI[] upgrades;

    [SerializeField] private ItemUI itemUI;
    [SerializeField] private TextMeshProUGUI txtNumberShard;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private Button btnAscend;

    [Inject] private PlayerCharacterManager playerCharacterManager;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private GameDataBase gameDataBase;

    private string currentCharacter = string.Empty;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardAscend;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardAscend;
    }

    private void OnEnable()
    {
        if (btnAscend != null)
            btnAscend.onClick.AddListener(OnBtnAscendClicked);
    }

    private void OnDisable()
    {
        if (btnAscend != null)
            btnAscend.onClick.RemoveListener(OnBtnAscendClicked);
    }

    private void OnBtnAscendClicked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return;

        var profile = playerCharacterManager.GetCharacter(currentCharacter);
        if (profile != null)
        {
            bool success = profile.StarUp();
            if (success)
            {
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardAscend(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }

    public void UpdateCharacterCardAscend(string id)
    {
        currentCharacter = id;
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = playerCharacterManager.GetCharacter(id).SaveData;

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(config.Rare);

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

        // Config item
        ItemConfig itemConfig = gameDataBase.GetItemConfig(id);
        if (itemConfig != null)
        {
            //int requiredShard = Utility.GetShardNeedToUpgradeAscend(data.StarUp + 1);
            //int ownShard = inventoryManager.GetItemQuantity(id);
            itemUI.Init(id, itemConfig.Rarity, itemConfig.Icon, gameDataBase.GetBGItemByRare(itemConfig.Rarity),0);
            itemUI.ActiveFragIcon(true); 
        }

        var profile = playerCharacterManager.GetCharacter(id);
        profile.GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQuantity);

        txtNumberShard.text = inventoryManager.GetItemQuantity(id).ToString() + "/" + requiredQuantity;
        txtCoin.text = Utility.FormatCurrency(requiredCoin);
    }
}
