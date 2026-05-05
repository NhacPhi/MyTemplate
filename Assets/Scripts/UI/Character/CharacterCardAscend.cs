using UnityEngine.UI;
using TMPro;
using UnityEngine;
using VContainer;

public class CharacterCardAscend : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;
    [SerializeField] private UpgradesUI upgrades;

    [SerializeField] private ItemUI itemUI;
    [SerializeField] private TextMeshProUGUI txtNumberShard;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private Button btnAscend;

    [Header("Skill Enhancement Preview")]
    [SerializeField] private SkillCharacterUI currentSkillUI;
    [SerializeField] private SkillCharacterUI nextSkillUI;

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

        var upgrader = playerCharacterManager.GetUpgradeManager(currentCharacter);
        if (upgrader != null)
        {
            bool success = upgrader.StarUp();
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
        txtLevel.text = data.Level.ToString() + "/" + Definition.MAX_CHARACTER_LEVEL.ToString();

        iconRare.sprite = gameDataBase.GetCharacterRareIcon(config.Rare);

        upgrades.UpdateUI(data.StarUp);

        // Config item
        ItemConfig itemConfig = gameDataBase.GetItemConfig(id);
        if (itemConfig != null)
        {
            //int requiredShard = Utility.GetShardNeedToUpgradeAscend(data.StarUp + 1);
            //int ownShard = inventoryManager.GetItemQuantity(id);
            itemUI.Init(id, itemConfig.Rarity, itemConfig.Icon, gameDataBase.GetBGItemByRare(itemConfig.Rarity),0);
            itemUI.ActiveFragIcon(true); 
        }

        var upgrader = playerCharacterManager.GetUpgradeManager(id);
        upgrader.GetNextStarUpRequirements(out int nextTier, out int requiredCoin, out int requiredQuantity);

        txtNumberShard.text = inventoryManager.GetItemQuantity(id).ToString() + "/" + requiredQuantity;
        txtCoin.text = Utility.FormatCurrency(requiredCoin);

        // Update skill enhancement preview
        UpdateSkillEnhancementPreview(config, data.StarUp);
    }

    /// <summary>
    /// Hiển thị skill sẽ được cường hóa khi star_up tiếp theo.
    /// currentSkillUI: hiển thị trạng thái hiện tại của skill sắp được nâng.
    /// nextSkillUI: hiển thị trạng thái sau khi nâng cấp.
    /// </summary>
    private void UpdateSkillEnhancementPreview(CharacterConfig config, int starUp)
    {
        int nextStarUp = Mathf.Min(starUp + 1, 6);

        // Tìm skill nào sẽ được cường hóa ở lần star_up tiếp theo
        SkillCharacter targetSkill = GetNextEnhancedSkill(starUp);

        // Lấy icon tương ứng
        Sprite skillIcon = targetSkill switch
        {
            SkillCharacter.Base     => config.BaseSkillIcon,
            SkillCharacter.Major    => config.MajorSkillIcon,
            SkillCharacter.Ultimate => config.UltimateSkillIcon,
            _ => config.BaseSkillIcon
        };

        int currentLevel = Utility.GetSkillEnhancementLevel(targetSkill, starUp);
        int nextLevel    = Utility.GetSkillEnhancementLevel(targetSkill, nextStarUp);

        // Current: trạng thái hiện tại của skill
        if (currentSkillUI != null)
        {
            currentSkillUI.SetSkillUI(skillIcon, currentLevel);
        }

        // Next: trạng thái sau khi nâng cấp
        if (nextSkillUI != null)
        {
            nextSkillUI.SetSkillUI(skillIcon, nextLevel);
        }
    }

    /// <summary>
    /// Xác định skill nào sẽ được cường hóa ở star_up tiếp theo.
    /// Thứ tự: Base → Major → Ultimate → Base → Major → Ultimate
    /// </summary>
    private SkillCharacter GetNextEnhancedSkill(int currentStarUp)
    {
        // star_up 0→1: Base, 1→2: Major, 2→3: Ultimate
        // star_up 3→4: Base, 4→5: Major, 5→6: Ultimate
        int nextIndex = currentStarUp % 3;
        return nextIndex switch
        {
            0 => SkillCharacter.Base,
            1 => SkillCharacter.Major,
            2 => SkillCharacter.Ultimate,
            _ => SkillCharacter.Base
        };
    }
}
